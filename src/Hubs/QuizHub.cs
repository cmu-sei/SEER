/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using System;
using System.Linq;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.SignalR;
using Seer.Infrastructure.Data;

namespace Seer.Hubs
{
    public class QuizHub : HubBase
    {
        private static readonly ConnectionMapping<string> _connections = new();

        public QuizHub(ApplicationDbContext context) : base(context) { }

        /// <summary>
        /// This doesn't change state, that is done in administration
        /// This does notify signalR clients that may be taking an active quiz
        /// (so that we can end quizzes)
        /// </summary>
        public async Task CurrentStateChanged(int quizId, Quiz.QuizStatus state)
        {
            foreach (var connectionId in _connections.GetConnections(quizId.ToString()))
            {
                await Clients.Client(connectionId).SendAsync("change", state.ToString());
            }
        }

        public async Task Hint(int quizId, int questionId, string hint)
        {
            var userId = Guid.Empty; //TODO

            var answer = this._context.Answers.FirstOrDefault(o => o.QuestionId == questionId) ?? new QuizAnswer();
            answer.HintTakenBy = this.UserId;
            answer.QuestionId = questionId;
            answer.UserId = userId.ToString();
            answer.Status = QuizAnswer.AnswerStateType.Pending;
            answer.Created = DateTime.UtcNow;
            this._context.Answers.Add(answer);
            await this._context.SaveChangesAsync();

            var question = this._context.Questions.FirstOrDefault(o => o.Id == questionId);
            if (question != null) hint = question.Hint;

            foreach (var connectionId in _connections.GetConnections(quizId.ToString()))
            {
                await Clients.Client(connectionId).SendAsync("hint", quizId, questionId, hint);
            }
        }

        public async Task AnswerChanged(string clientUuid, string who, string quizid, string questionId, string newAnswer, string newAnswerString)
        {
            var uid = this.UserId;
            var name = this.UserName;

            var answer = new QuizAnswer
            {
                QuestionId = Convert.ToInt32(questionId),
                UserId = uid,
                AnsweredIndex = Convert.ToInt32(newAnswer),
                AnsweredText = newAnswerString,
                Status = QuizAnswer.AnswerStateType.Pending,
                Created = DateTime.UtcNow
            };
            this._context.Answers.Add(answer);
            await this._context.SaveChangesAsync();


            //notify those in the group that are connected
            foreach (var connectionId in _connections.GetConnections(quizid))
            {
                _log.Debug($"Data sent: Quiz: {quizid} Name:{name} ConnId:{Context.ConnectionId}");
                await Clients.Client(connectionId).SendAsync("AnswerChanged", clientUuid, name, quizid, questionId, newAnswer, newAnswerString);
            }
        }

        public async Task Close(int quizId, Guid userId)
        {
            var quiz = this._context.Quizzes.FirstOrDefault(o => o.Id == quizId);
            if (quiz != null)
            {
                quiz.Status = Quiz.QuizStatus.Closed;
                await this._context.SaveChangesAsync();

                //History.HistoryManager.Save(new History.HistoryItem { Key = History.HistoryItem.HistoryKey.SubmittedQuiz, UserId = Guid.Parse(this._userId), Value = quizId.ToString() });
                _log.Debug($"Quiz submitted: {quizId} by: {userId}");
                foreach (var connectionId in _connections.GetConnections(quizId.ToString()))
                {
                    await Clients.Client(connectionId).SendAsync("close", quizId);
                }
            }

        }

        public override Task OnConnectedAsync()
        {
            var name = this.UserId;
            var userId = Guid.Empty;


            var quizId = Convert.ToInt32(this.GetHttpContext().Request.Query["quizid"].ToString());
            _connections.Add(quizId.ToString(), Context.ConnectionId);
            _log.Debug($"Hub connection: Quiz: {quizId} Name:{name} ConnId:{Context.ConnectionId}");

            this._context.QuizConnections.Add(new QuizConnection(quizId, userId.ToString()));
            this._context.SaveChanges();

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var name = this.UserId;
            var quizId = Convert.ToInt32(this.GetHttpContext().Request.Query["quizid"].ToString());
            var userId = Guid.Empty;

            _connections.Remove(name, Context.ConnectionId);
            _log.Debug($"Hub disconnect: Quiz: {quizId} Name:{name} ConnId:{Context.ConnectionId}");

            var connections = this._context.QuizConnections.Where(o => o.UserId == userId.ToString() && o.QuizId == quizId);
            foreach (var connection in connections)
            {
                this._context.QuizConnections.Remove(connection);
            }
            this._context.SaveChanges();

            return base.OnDisconnectedAsync(exception);
        }
    }
}
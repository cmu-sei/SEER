/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("asp_net_users");
            });

            modelBuilder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.ToTable("asp_net_user_claims");
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.ToTable("asp_net_user_logins");
            });

            modelBuilder.Entity<IdentityUserToken<string>>(b =>
            {
                b.ToTable("asp_net_user_tokens");
            });

            modelBuilder.Entity<IdentityRole>(b =>
            {
                b.ToTable("asp_net_roles");
            });

            modelBuilder.Entity<IdentityRoleClaim<string>>(b =>
            {
                b.ToTable("asp_net_role_claims");
            });

            modelBuilder.Entity<IdentityUserRole<string>>(b =>
            {
                b.ToTable("asp_net_user_roles");
            });

            modelBuilder.Entity<Announcement>().HasIndex(o => new { o.AssessmentId });
            modelBuilder.Entity<AssessmentTime>().HasIndex(o => new { o.AssessmentId });
            modelBuilder.Entity<AssessmentTimesHistory>().HasIndex(o => new { o.AssessmentTimeId });

            modelBuilder.Entity<Campaign>().HasIndex(o => new { o.Id });
            modelBuilder.Entity<CampaignDataPoint>().HasIndex(o => new { o.Id });
            modelBuilder.Entity<Document>().HasIndex(o => new { o.AssessmentId });

            modelBuilder.Entity<Event>().HasIndex(o => new { o.Id });
            modelBuilder.Entity<Event>().HasIndex(o => new { o.AssessmentId });

            modelBuilder.Entity<EventDetail>().HasIndex(o => new { o.Id });

            modelBuilder.Entity<EventDetailHistory>().HasIndex(o => new { o.Id });
            modelBuilder.Entity<EventDetailHistory>().HasIndex(o => new { o.EventId });
            modelBuilder.Entity<EventDetailHistory>().HasIndex(o => new { o.EventDetailId });
            modelBuilder.Entity<EventDetailHistory>().HasIndex(o => new { o.HistoryAction });
            modelBuilder.Entity<EventDetailHistory>().HasIndex(o => new { o.Message });
            modelBuilder.Entity<EventDetailHistory>().HasIndex(o => new { o.AssessmentId });
            modelBuilder.Entity<EventDetailHistory>().HasIndex(o => new { o.Status });
            modelBuilder.Entity<EventDetailHistory>().HasIndex(o => new { o.Created });
            modelBuilder.Entity<EventDetailHistory>().HasIndex(o => new { o.IntegrationId });

            modelBuilder.Entity<Faq>().HasIndex(o => new { o.AssessmentId });
            modelBuilder.Entity<GroupUser>().HasIndex(o => new { o.GroupId });
            modelBuilder.Entity<GroupUser>().HasIndex(o => new { o.UserId });
            modelBuilder.Entity<History.HistoryItem>().HasIndex(o => new { o.UserId });
            modelBuilder.Entity<IntelItem>().HasIndex(o => new { o.AssessmentId });

            modelBuilder.Entity<Survey>().HasIndex(o => new { o.AssessmentId });
            modelBuilder.Entity<SurveyQuestion>().HasIndex(o => new { o.SurveyId });
            modelBuilder.Entity<SurveyAnswer>().HasIndex(o => new { o.QuestionId });

            modelBuilder.Entity<MET>().HasIndex(o => new { o.AssessmentId });
            modelBuilder.Entity<Operation>().HasIndex(o => new { o.Id });

            modelBuilder.Entity<Quiz>().HasIndex(o => new { o.AssessmentId });
            modelBuilder.Entity<QuizConnection>().HasIndex(o => new { o.QuizId });
            modelBuilder.Entity<QuizQuestion>().HasIndex(o => new { o.QuizId });
            modelBuilder.Entity<QuizQuestion>().Property(p => p.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<QuizAnswer>().HasIndex(o => new { o.QuestionId });

            modelBuilder.Entity<Section>().HasIndex(o => new { o.AssessmentId });

            modelBuilder.Entity<TaskingItem>().HasIndex(o => new { o.AssessmentId });

            modelBuilder.Entity<Upload>().HasIndex(o => new { o.AssessmentId });

            modelBuilder.Entity<UserJwt>().HasIndex(o => new { o.SsoId });
        }

        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<QuizAnswer> Answers { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<AssessmentTime> AssessmentTime { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignDataPoint> CampaignDataPoints { get; set; }
        public DbSet<AssessmentEventCatalogItemDetail> CatalogEventDetails { get; set; }
        public DbSet<AssessmentEventCatalogItem> CatalogEvents { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventDetail> EventDetails { get; set; }
        public DbSet<EventDetailHistory> EventDetailHistory { get; set; }
        public DbSet<Faq> Faqs { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupUser> GroupUsers { get; set; }
        public DbSet<History.HistoryItem> History { get; set; }
        public DbSet<IntelItem> IntelItems { get; set; }
        public DbSet<MET> METs { get; set; }
        public DbSet<METItem> METItems { get; set; }
        public DbSet<METItemSCT> METScts { get; set; }
        public DbSet<METItemSCTScore> METItemSCTScores { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> Questions { get; set; }
        public DbSet<QuizConnection> QuizConnections { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
        public DbSet<SurveyAnswer> SurveyAnswers { get; set; }
        public DbSet<TaskingItem> TaskingItems { get; set; }
        public DbSet<TaskingItemResult> TaskingItemResults { get; set; }
        public DbSet<Upload> Uploads { get; set; }
        public DbSet<UserJwt> UserJwts { get; set; }
    }
}
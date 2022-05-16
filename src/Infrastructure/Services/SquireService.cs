/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.Services;

public class SquireService
{
    private static readonly Logger _log = LogManager.GetCurrentClassLogger();
    private readonly ApplicationDbContext _db;
    private IdentityToken _token = null!;

    public SquireService(ApplicationDbContext dbContext)
    {
        this._db = dbContext;
    }

    public async Task Run(User user)
    {
        _log.Info($"Checking if user {user} is Squired account");
        
        //get account to Squire
        var squiredAccount = await GetSquiredAccount(user);
        if (squiredAccount != null)
        {
            //if the account is squired, then add them to the appropriate teams in player and mattermost
            _log.Info($"{user} is Squired account");
            
            //get group
            var squireIntegration = await this._db.SquireIntegrations.FirstOrDefaultAsync(x => x.GroupId == squiredAccount.GroupId);
            
            //add to player
            await PlayerGetToken();
            await PlayerCreateAccount(user);
            await PlayerAddUserToTeam(squiredAccount, squireIntegration);

            //add to mattermost/rocketchat/chat framework we decide to use

            //update SquiredAccount record with OAuthID to indicate it is complete
            squiredAccount.OAuthId = user.OAuthId;
            this._db.Update(squiredAccount);
            await this._db.SaveChangesAsync();
        }
    }

    private async Task PlayerCreateAccount(User user)
    {
        _log.Info($"Creating player user {user.Name()}...");
        var client = new RestClient($"{Program.Configuration.Player.BaseUrl}/api/users");
        var request = new RestRequest { Method = Method.Post, Timeout = -1 };
        request.AddHeader("Authorization", $"Bearer {_token.AccessToken}");
        var body = @"{" + "\n" +
                   @"    ""id"": """+ user.OAuthId + ",\n" +
                   @"    ""name"": """+ user.Name() + "\n" +
                   @"}";
        request.AddParameter("application/json", body,  ParameterType.RequestBody);
        try
        {
            await client.ExecuteAsync(request);
            _log.Info($"Player user created successfully.");
        }
        catch (Exception e)
        {
            _log.Error($"Failed to create player user: {user.Name()} with: {e}");
        }
    }
    
    async Task PlayerAddUserToTeam(SquiredAccount squiredAccount, SquireIntegrations? squireIntegrations)
    {
        _log.Info($"Adding user {squiredAccount.MatchName} to team...");
        if (squireIntegrations != null)
        {
            var client = new RestClient($"{Program.Configuration.Player.BaseUrl}/api/teams/{squireIntegrations.GroupId}/users/{squiredAccount.Id}");
            var request = new RestRequest { Method = Method.Post, Timeout = -1 };
            request.AddHeader("Authorization", $"Bearer {_token.AccessToken}");
            try
            {
                await client.ExecuteAsync(request);
                _log.Info($"User added to team successfully.");
            }
            catch (Exception e)
            {
                _log.Error($"Failed to add user: {squiredAccount.MatchName} to team: {squireIntegrations.PlayerTeamId} with: {e}");
            }
        }
    }

    private async Task PlayerGetToken()
    {
        _log.Info("Getting API token...");
        
        var client = new RestClient($"{Program.Configuration.AuthenticationAuthority}/connect/token");
        var request = new RestRequest { Method = Method.Post, Timeout = -1 };
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddParameter("grant_type", "password");
        request.AddParameter("username", Program.Configuration.Player.Username);
        request.AddParameter("password", Program.Configuration.Player.Password);
        request.AddParameter("client_secret", Program.Configuration.Player.ClientSecret);
        request.AddParameter("client_id", Program.Configuration.Player.ClientId);
        request.AddParameter("scope", Program.Configuration.Player.Scope);
        var response = await client.ExecuteAsync(request);
        if (response.Content == null)
        {
            const string msg = "Token call did not return data";
            _log.Error(msg);
            throw new Exception(msg);
        }

        _token = JsonConvert.DeserializeObject<IdentityToken>(response.Content)!;
        _log.Info("Token acquired.");
    }


    private IEnumerable<string> GetPossibleNames(User user)
    {
        _log.Info($"Building a list of possible name matches...");
        var possibleNames = new List<string>();
        
        var userName = user.Name().Trim();
        if (!string.IsNullOrEmpty(userName))
        {
            possibleNames.Add(userName);
            if (userName.Contains(' '))
            {
                possibleNames.Add(userName.Replace(" ", "."));
            }
        }    
        if (!string.IsNullOrEmpty(user.Email.Trim()))
        {
            var email = user.Email.Trim();
            possibleNames.Add(email);
            var emailArray = email.Split('@');
            possibleNames.Add(emailArray[0]);
        }
        
        _log.Info($"Built list of {possibleNames.Count} potential matches.");
        
        return possibleNames;
    }

    private async Task<SquiredAccount?> GetSquiredAccount(User user)
    {
        var possibleNames = GetPossibleNames(user);
        _log.Info($"Checking if this user is a squired account...");
        foreach (var possibleName in possibleNames)
        {
            var x = await this._db.SquiredAccounts.FirstOrDefaultAsync(x => x.MatchName.ToLower().Contains(possibleName.ToLower()));
            if (x != null) 
                return x;
        }
        return null;
    }
}
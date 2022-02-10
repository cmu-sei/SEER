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
using System.Collections.Generic;
using System.Linq;
using NLog;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.Services;

public class SquireService
{
    private static readonly Logger _log = LogManager.GetCurrentClassLogger();
    private ApplicationDbContext _db;

    public SquireService(ApplicationDbContext dbContext)
    {
        this._db = dbContext;
    }

    public void Enable(string matchName)
    {
        foreach (var squireAccount in GetSquireAccounts(matchName))
        {
            //get player api token
            //add user to player
            //add user to group
            
            //get chat api 
            //add to chat
            
            //update locally
        }
    }

    private IEnumerable<SquireAccount> GetSquireAccounts(string matchName)
    {
        //normalize matchName?
        
        return this._db.SquireAccounts.Where(x => x.MatchName.Equals(matchName, StringComparison.InvariantCultureIgnoreCase));
    }
}
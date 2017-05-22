using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class JoinAliasIntIssue : OrmLiteTestBase
    {
        class Team
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public int? TeamLeaderId { get; set; }
        }

        class TeamUser
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public int TeamId { get; set; }
        }

        [Test]
        public void Can_create_query_with_int_JoinAlias()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TeamUser>();
                db.DropAndCreateTable<Team>();

                db.InsertAll(new[] {
                    new Team
                    {
                        Id = 1,
                        Name = "Team 1"
                    },
                });

                db.InsertAll(new[]
                {
                    new TeamUser
                    {
                        Id = 1,
                        Name = "User 1",
                        TeamId = 1
                    },
                    new TeamUser
                    {
                        Id = 2,
                        Name = "User 2",
                        TeamId = 1
                    },
                });

                db.UpdateOnly(new Team { TeamLeaderId = 1 }, 
                    onlyFields: x => x.TeamLeaderId, 
                    where: x => x.Id == 1);

                var q = db.From<Team>();
                q.Join<TeamUser>((t, u) => t.Id == u.TeamId, db.JoinAlias("TeamUser"));
                q.Join<TeamUser>((t, u) => t.TeamLeaderId == u.Id, db.JoinAlias("Leader"));
                q.Where<Team, TeamUser>((t, u) => t.Id == Sql.JoinAlias(u.TeamId, "Leader"));
                q.Where<TeamUser>(u => Sql.JoinAlias(u.Id, "Leader") == 1);
                q.Where<Team, TeamUser>((t, u) => Sql.JoinAlias(t.Id, OrmLiteConfig.DialectProvider.GetQuotedTableName(ModelDefinition<Team>.Definition)) == Sql.JoinAlias(u.TeamId, "Leader")); // Workaround, but only works for fields, not constants
                q.Where<Team, TeamUser>((user, leader) => Sql.JoinAlias(user.Id, "TeamUser") < Sql.JoinAlias(leader.Id, "Leader"));
                q.Select<Team, TeamUser, TeamUser>((t, u, l) => new
                {
                    TeamName = Sql.As(t.Name, "TeamName"),
                    UserName = Sql.As(u.Name, "UserName"),
                    LeaderName = Sql.As(l.Name, "LeaderName")
                });

                var results = db.Select<dynamic>(q);
            }
        }

    }
}
using System.Collections.Generic;

//Apache 2.0 License: https://github.com/StackExchange/dapper-dot-net/blob/master/License.txt
namespace ServiceStack.OrmLite.Dapper
{
    partial class SqlMapper
    {
        /// <summary>
        /// Represents a placeholder for a value that should be replaced as a literal value in the resulting sql
        /// </summary>
        internal struct LiteralToken
        {
            /// <summary>
            /// The text in the original command that should be replaced
            /// </summary>
            public string Token { get; set; }

            /// <summary>
            /// The name of the member referred to by the token
            /// </summary>
            public string Member { get; set; }

            internal LiteralToken(string token, string member)
            {
                Token = token;
                Member = member;
            }

            internal static readonly IList<LiteralToken> None = new LiteralToken[0];
        }
    }
}

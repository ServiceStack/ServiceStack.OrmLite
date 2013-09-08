using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using ServiceStack.Common.Utils;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;

namespace SqliteExpressionsTest
{
    public class Author
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [AutoIncrement]
        [Alias("AuthorID")]
        public Int32 Id { get; set;}

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Index(Unique = true)]
        [StringLength(40)]
        public string Name { get; set;}

        /// <summary>
        /// Gets or sets the birthday.
        /// </summary>
        public DateTime Birthday { get; set;}

        /// <summary>
        /// Gets or sets the last activity.
        /// </summary>
        public DateTime? LastActivity  { get; set;}

        /// <summary>
        /// Gets or sets the earnings.
        /// </summary>
        public Decimal? Earnings { get; set;}

        /// <summary>
        /// Gets or sets a value indicating whether active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        [StringLength(80)]
        [Alias("JobCity")]
        public string City { get; set;}

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        [StringLength(80)]
        [Alias("Comment")]
        public string Comments { get; set;}

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        public Int16 Rate{ get; set;}


    }

    
}
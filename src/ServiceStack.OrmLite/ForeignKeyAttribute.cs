﻿using System;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : ReferencesAttribute
    {
        public ForeignKeyAttribute(Type type) : base(type)
        {
        }

        public string OnDelete { get; set; }
        public string OnUpdate { get; set; }
    }
}

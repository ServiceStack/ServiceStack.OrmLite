using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.Tests.Shared
{
    public class Person
    {
        public static Person[] Rockstars = new[] {
            new Person(1, "Jimi", "Hendrix", 27),
            new Person(2, "Janis", "Joplin", 27),
            new Person(3, "Jim", "Morrisson", 27),
            new Person(4, "Kurt", "Cobain", 27),
            new Person(5, "Elvis", "Presley", 42),
            new Person(6, "Michael", "Jackson", 50),
        };

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }

        public Person() { }
        public Person(int id, string firstName, string lastName, int age)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Age = age;
        }

        protected bool Equals(Person other)
        {
            return Id == other.Id &&
                string.Equals(FirstName, other.FirstName) &&
                string.Equals(LastName, other.LastName) &&
                Age == other.Age;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Person)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ (FirstName != null ? FirstName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LastName != null ? LastName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Age;
                return hashCode;
            }
        }
    }

    public class PersonWithAutoId
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }

    public class PersonWithNullableAutoId
    {
        [AutoIncrement]
        public int? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }

    public class EntityWithId
    {
        public int Id { get; set; }
    }


    public class PersonWithAliasedAge
    {
        [PrimaryKey]
        public string Name { get; set; }

        [Alias("YearsOld")]
        public int Age { get; set; }

        public string Ignored { get; set; }
    }

    public class PersonUsingEnumAsInt
    {
        public string Name { get; set; }
        public Gender Gender { get; set; }
    }

    [EnumAsInt]
    public enum Gender
    {
        Unknown = 0,
        Female,
        Male
    }

}
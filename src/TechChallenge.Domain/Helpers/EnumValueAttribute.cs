namespace TechChallenge.Domain.Helpers
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class EnumValueAttribute : Attribute
    {
        public string Value { get; }
        public string[] Values { get; set; }
        public string Description { get; }

        public EnumValueAttribute(string value, string description = "")
        {
            Value = value;
            Values = new string[] { value };
            Description = description;
        }

        public EnumValueAttribute(string[] values, string description = "")
        {
            Value = values[0];
            Values = values;
            Description = description;
        }
    }
}

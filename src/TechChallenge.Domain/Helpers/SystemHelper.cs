namespace TechChallenge.Domain.Helpers
{
    public static class SystemHelper
    {
        public static string GetStatusDescription<TEnum>(TEnum status) where TEnum : struct, Enum
        {
            var attribute = ((Enum)(object)status).GetAttribute<EnumValueAttribute>();

            if (attribute is null)
                return string.Empty;

            return attribute?.Description ?? string.Empty;
        }

        public static IEnumerable<KeyValuePair<string, string>> GetEnumValueAndDescription<TEnum>() where TEnum : struct, Enum
        {
            return from TEnum item in Enum.GetValues(typeof(TEnum)).Cast<TEnum>()
                   let att = ((Enum)(object)item).GetAttribute<EnumValueAttribute>()
                   where att != null
                   select new KeyValuePair<string, string>(att.Value, att.Description);
        }
    }
}

using System.ComponentModel;

namespace M11.Common.Attributes
{
    public class FullDescriptionAttribute : DescriptionAttribute
    {
        public FullDescriptionAttribute(string description, string fullDescription) : base(description)
        {
            FullDescription = fullDescription;
        }

        public string FullDescription { get; }
    }
}

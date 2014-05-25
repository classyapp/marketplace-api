using Nest;

namespace Classy.Models.Keywords
{
    [ElasticType(Name = "keyword")]
    public class KeywordIndexDto
    {
        [ElasticProperty(Index = FieldIndexOption.not_analyzed)]
        public string Keyword { get; set; }

        public string Language { get; set; }

        public int Count { get; set; }
    }
}
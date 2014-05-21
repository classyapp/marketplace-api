using System;
using System.Linq;
using Classy.Interfaces.Search;
using Classy.Models.Search;
using Nest;

namespace classy.Manager.Search
{
    public class ProfileSearchProvider : IProfileSearchProvider
    {
        private const string IndexName = "profiles";
        private readonly ISearchClientFactory _searchClientFactory;

        public ProfileSearchProvider(ISearchClientFactory searchClientFactory)
        {
            _searchClientFactory = searchClientFactory;
        }

        public SearchResults<ProfileIndexDto> Search(string query, string country, string appId, int amount = 25, int page = 1)
        {
            // This elasticsearch query currently uses 'script_score' in the 'function_score' method
            // When NEST starts supporting 'field_value_factor', then we should convert this query
            // to use that, since it should be much faster in performance.
            // http://www.elasticsearch.org/guide/en/elasticsearch/reference/current/mapping-boost-field.html
            // https://github.com/elasticsearch/elasticsearch/pull/5519

            //var boosted = GetBoostedResults(query, appId);

            var client = _searchClientFactory.GetClient(IndexName, appId);

            // parameter factors should be from global settings (and able to change easily)
            var script = "_score + ((doc['reviewAverageScore'].value - 2.5) * 2)" +
                         " + (doc['viewCount'].value / 100)" +
                         " + (doc['followerCount'].value / 3)";

            var descriptor = new SearchDescriptor<ProfileIndexDto>()
                .Query(q => q.FunctionScore(
                    fs => fs.Query(
                        qq => qq.Bool(
                            b => b.Should(m => m.Term(t => t.Country, country))
                                .Must(mq => mq.QueryString(
                                    qs => qs.OnFields(f => f.AnalyzedCompanyName, f => f.Metadata)
                                        .Query(query))
                                )
                        )
                    )
                    .Functions(
                        ff => ff.ScriptScore(
                            ss => ss.Script(script)
                            )
                    )
                ));

            descriptor
                .Size(amount)
                .From(amount * (page - 1));

            // find a better way (than precompiler flags) to log if we have problems...
            var request = client.Serializer.Serialize(descriptor);

            var response = client.Search<ProfileIndexDto>(_ => descriptor);

            return new SearchResults<ProfileIndexDto> {
                Results = response.Documents.ToList(),
                TotalResults = Convert.ToInt32(response.Total)
            };
        }

        private object GetBoostedResults(string query, string appId)
        {
            // TODO: implement logic!
            return null;
        }
    }
}
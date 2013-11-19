using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace Refactorings
{
    public class WordLookupService
    {
        //static FSDirectory directory = FSDirectory.Open(new DirectoryInfo(Path.GetTempPath()));
        static FSDirectory directory = FSDirectory.Open(new DirectoryInfo(@"C:\Users\Chris\Desktop\Lucene\"));

        public void Initialize()
        {
            using (var analyzer = new StandardAnalyzer(Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.LIMITED))
            using (var reader = File.OpenText(@"C:\Users\Chris\Desktop\US.dic"))
            {
                string word;
                while ((word = reader.ReadLine()) != null)
                {
                    var doc = new Document();
                    doc.Add(new Field("word", word, Field.Store.YES, Field.Index.ANALYZED));
                    writer.AddDocument(doc);
                }
            }
        }

        public IEnumerable<string> Search(string searchQuery)
        {
            int hits_limit = 5;

            using (var searcher = new IndexSearcher(directory, false))
            using (var analyzer = new StandardAnalyzer(Version.LUCENE_30))
            {
                var parser = new QueryParser(Version.LUCENE_30, "word", analyzer);
                var query = new FuzzyQuery(new Term("word", searchQuery), 0.5f);
                var docs = searcher.Search(query, null, hits_limit, Sort.RELEVANCE);

                foreach (var hit in docs.ScoreDocs)
                {
                    var doc = searcher.Doc(hit.Doc);
                    yield return doc.Get("word");
                }
            }
        }

        public bool SearchExact(string searchQuery)
        {
            const int hits_limit = 1;

            using (var searcher = new IndexSearcher(directory, true))
            using (var analyzer = new StandardAnalyzer(Version.LUCENE_30))
            {
                var parser = new QueryParser(Version.LUCENE_30, "word", analyzer);
                var query = new TermQuery(new Term("word", searchQuery));
                var hits = searcher.Search(query, hits_limit).ScoreDocs;
                
                foreach (var hit in hits)
                {
                    var doc = searcher.Doc(hit.Doc);
                    string docWord = doc.Get("word");

                    if (docWord.Equals(searchQuery, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }

                return false;
            }
        }
    }
}

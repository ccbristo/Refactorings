using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace Refactorings
{
    public class WordLookupService
    {
        //static FSDirectory directory = FSDirectory.Open(new DirectoryInfo(Path.GetTempPath()));
        static FSDirectory directory = FSDirectory.Open(new DirectoryInfo(@"C:\Users\Chris\Desktop\Lucene\"));

        private class LuceneSearchHandle : IDisposable
        {
            public IndexSearcher Searcher { get; private set; }

            public LuceneSearchHandle(IndexSearcher searcher)
            {
                this.Searcher = searcher;
            }

            ~LuceneSearchHandle()
            {
                this.Dispose(false);
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (Searcher != null)
                    Searcher.Dispose();

                Searcher = null;
            }
        }

        private LuceneSearchHandle SearchHandle;

        public WordLookupService()
        {
            var searcher = new IndexSearcher(directory, true);
            SearchHandle = new LuceneSearchHandle(searcher);
        }

        public void Initialize()
        {
            using (var analyzer = new StandardAnalyzer(Version.LUCENE_30, new HashSet<string>()))
            using (var writer = new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.LIMITED))
            using(var wordStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Refactorings.words.txt"))
            using (var reader = new StreamReader(wordStream))
            {
                string word;
                while ((word = reader.ReadLine()) != null)
                {
                    var doc = new Document();
                    doc.Add(new Field("word", word, Field.Store.YES, Field.Index.ANALYZED));
                    writer.AddDocument(doc);
                }

                writer.Optimize();
            }
        }

        public IEnumerable<string> Search(string searchQuery)
        {
            const int Hits_Limit = 5;

            var timer = System.Diagnostics.Stopwatch.StartNew();

            var query = new FuzzyQuery(new Term("word", searchQuery), 0.5f);
            var docs = SearchHandle.Searcher.Search(query, null, Hits_Limit, Sort.RELEVANCE);

            foreach (var hit in docs.ScoreDocs)
            {
                var doc = SearchHandle.Searcher.Doc(hit.Doc);
                yield return doc.Get("word");
            }

            timer.Stop();
            var elapsed = timer.Elapsed;
        }

        public bool SearchExact(string searchQuery)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var query = new TermQuery(new Term("word", searchQuery));
            var hits = SearchHandle.Searcher.Search(query, 1).ScoreDocs;

            timer.Stop();
            var x = timer.Elapsed;
            return hits.Length > 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestObserver
{
    public class BlogUser : IObservable<object>
    {
        private List<IObserver<object>> observers;
        public BlogUser()
        {
            observers = new List<IObserver<object>>();
        }
        public void publishBlog(String articleTitle, String articleContent)
        {
            Article art = new Article();
            art.setArticleTitle(articleTitle);
            art.setArticleContent(articleContent);
            Console.WriteLine("博主:发表新文章，文章标题:" + articleTitle + ",文章内容:" + articleContent);
            foreach (var observer in observers)
            {
                 observer.OnNext(art);
            }
        }
        public IDisposable Subscribe(IObserver<object> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
            return new Unsubscriber(observers, observer);
        }
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<object>> _observers;
            private IObserver<object> _observer;

            public Unsubscriber(List<IObserver<object>> observers, IObserver<object> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }
        public void EndTransmission()
        {
            foreach (var observer in observers.ToArray())
                if (observers.Contains(observer))
                    observer.OnCompleted();

            observers.Clear();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestObserver
{
    public class MyObserver : IObserver<object>
    {
        private IDisposable unsubscriber;

        public virtual void Subscribe(IObservable<object> provider)
        {
            if (provider != null)
                unsubscriber = provider.Subscribe(this);
        }

        public virtual void OnCompleted()
        {
            Console.WriteLine("Completed");
            this.Unsubscribe();
        }

        public virtual void OnError(Exception e)
        {
            Console.WriteLine(e.Message);
        }

        public virtual void OnNext(object obj)
        {
            Article art = obj as Article;
            Console.WriteLine("博主发表了新的文章，快去看吧！");
            Console.WriteLine("博客标题为：" + art.getArticleTitle());
            Console.WriteLine("博客内容为：" + art.getArticleContent());
        }

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }
    }
}

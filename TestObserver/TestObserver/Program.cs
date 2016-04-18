using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace TestObserver
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            BlogUser user = new BlogUser();
            user.Subscribe(new MyObserver());
            user.publishBlog("哈哈，博客上线了", "大家多来访问");
            Console.ReadKey();
        }
    }
}

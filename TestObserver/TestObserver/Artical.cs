using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestObserver
{
    public class Article
    {
        private String articleTitle;
        private String articleContent;

        public String getArticleTitle()
        {
            return articleTitle;
        }

        public void setArticleTitle(String articleTitle)
        {
            this.articleTitle = articleTitle;
        }

        public String getArticleContent()
        {
            return articleContent;
        }

        public void setArticleContent(String articleContent)
        {
            this.articleContent = articleContent;
        }
    }
}

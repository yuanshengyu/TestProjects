using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

[Serializable]
[Microsoft.SqlServer.Server.SqlUserDefinedType(Format.Native)]
public struct SqlCoordinate : INullable
{
    private int longitude;
    private int latitude;
    private bool isNull;
    public override string ToString()
    {
        // 用您的代码替换下列代码
        return "";
    }

    public bool IsNull
    {
        get
        {
            // 在此处放置代码
            return m_Null;
        }
    }

    public static SqlCoordinate Null
    {
        get
        {
            SqlCoordinate h = new SqlCoordinate();
            h.m_Null = true;
            return h;
        }
    }

    public static SqlCoordinate Parse(SqlString s)
    {
        if (s.IsNull)
            return Null;
        SqlCoordinate u = new SqlCoordinate();
        // 在此处放置代码
        return u;
    }

    // 这是占位符方法
    public string Method1()
    {
        //在此处插入方法代码
        return "Hello";
    }

    // 这是占位符静态方法
    public static SqlString Method2()
    {
        //在此处插入方法代码
        return new SqlString("Hello");
    }

    // 这是占位符字段成员
    public int var1;
    // 私有成员
    private bool m_Null;
}

public enum Orientation
{
    NorthEast,
    NorthWest,
    SouthEast,
    SouthWest
}



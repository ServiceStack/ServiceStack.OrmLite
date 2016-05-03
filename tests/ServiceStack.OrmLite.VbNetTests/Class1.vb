Imports System
Imports ServiceStack
Imports ServiceStack.Text
Imports ServiceStack.OrmLite

Module Module1

    Public Class Poco
        Public Property Id As Integer
        Public Property Name As String
    End Class

    Sub Main()
        Console.Write("Hello VB.NET ")
        Console.Write(Env.VersionString)

        Dim dbFactory As New OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider)

        Dim db As IDbConnection = dbFactory.Open()

        db.DropAndCreateTable(Of Poco)()

        Dim row As New Poco()
        row.Id = 1
        row.Name = "Foo"

        db.Insert(row)

        Dim q As SqlExpression(Of Poco) = db.From(Of Poco)().Where(Function(x) x.Name <> "Bar")
        Dim rows As List(Of Poco) = db.Select(q)
        rows.PrintDump()

        Console.WriteLine("<> ''")

        q = db.From(Of Poco)().Where(Function(x) x.Name <> "")
        rows = db.Select(q)
        rows.PrintDump()

        Console.ReadLine()
    End Sub

End Module

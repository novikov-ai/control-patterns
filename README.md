# control-patterns

### [Example 1 - Writer](Example1/Writer.cs)

В данном примере для записи символов в поток используется `StreamWriter`. При этом данный класс инициируется в структуре try-catch-finally, где в блоке finally мы закрываем поток, чтобы освободить неуправляемые ресурсы. 

Абстракция `using (... = new StreamWriter(...))` позволяет инкапсулировать данную логику, но компактнее. При этом мы страхуемся от ошибок, когда можем забыть явно вызвать метод `Close` для освобождения ресурсов.

Было:
~~~C#
public static void CreateFile(string sharedFileParameter, List<IGrouping<string, ParsedParameter>> groups)
{
    var outputFile = $"{Path.GetDirectoryName(sharedFileParameter)}\\Exported_{DateTime.Now:yyyyMMdd}.cs";

    var sw = new StreamWriter(outputFile);

    try
    {
        sw.Write(NamespaceDeclaration);

        var groupCount = 0;
        foreach (var group in groups)
        {
            var parameterCount = 0;

            sw.WriteLine($@"[{GroupAttributeName}(""{group.Key}"")]
public static class {BasicNameGroup}{groupCount}{{");

            foreach (ParsedParameter fspParameter in group)
            {
                sw.WriteLine($@"/// <summary>
/// {fspParameter.Name}
/// </summary>
public static readonly {ParameterConstantClassName} {BasicNameParameter}{parameterCount} =
new {ParameterConstantClassName}(""{fspParameter.Name}"", new Guid(""{fspParameter.Guid}""));");

                parameterCount++;
            }

            sw.WriteLine('}');
            groupCount++;
        }

        sw.WriteLine(EndBrackets);
    }
    finally
    {
        sw.Close();
    }
}
~~~

Стало:
~~~C#
public static void CreateFileV2(string sharedFileParameter, List<IGrouping<string, ParsedParameter>> groups)
{
    var outputFile = $"{Path.GetDirectoryName(sharedFileParameter)}\\Exported_{DateTime.Now:yyyyMMdd}.cs";

    using (var sw = new StreamWriter(outputFile))
    {
        sw.WriteLine(NamespaceDeclaration);

        var groupCount = 0;
        foreach (var group in groups)
        {
            var parameterCount = 0;

            sw.WriteLine($@"[{GroupAttributeName}(""{group.Key}"")]
public static class {BasicNameGroup}{groupCount}{{");

            foreach (ParsedParameter fspParameter in group)
            {
                sw.WriteLine($@"/// <summary>
/// {fspParameter.Name}
/// </summary>
public static readonly {ParameterConstantClassName} {BasicNameParameter}{parameterCount} =
new {ParameterConstantClassName}(""{fspParameter.Name}"", new Guid(""{fspParameter.Guid}""));");

                parameterCount++;
            }

            sw.WriteLine('}');
            groupCount++;
        }

        sw.WriteLine(EndBrackets);
    }
}
~~~

### [Example 2](Example2/Reader.cs)

Как и в примере №1, абстракция `using (... = new StreamReader(...))` позволяет инкапсулировать логику по инициализации потока и безопасного удаления ресурсов после использования.

Было:
~~~C#
public static void Read(string path)
{
    StreamReader sr = new StreamReader(path);
    try
    {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            Console.WriteLine(line);
        }

    }
    catch (Exception e)
    {
        Console.WriteLine("The file could not be read:");
        Console.WriteLine(e.Message);
    }
    finally
    {
        sr.Close();
    }
}
~~~

Стало:
~~~C#
public static void ReadV2(string path)
{
    try
    {
        using (StreamReader sr = new StreamReader(path))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine("The file could not be read:");
        Console.WriteLine(e.Message);
    }
}
~~~

### [Example 3](Example3/Service.cs)

Избавляемся от лишних комментариев коде и добавляем безопасную конструкцию по освобождению ресурсов. 

Было:
~~~C#
private static void HttpRequestLoop(string query)
{
    // Setup ServicePointManager to handle http verfication
    // Source: https://stackoverflow.com/a/2904963
    ServicePointManager.Expect100Continue = true;
    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    // Construct request
    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);
    request.MaximumAutomaticRedirections = Redirects;
    request.AutomaticDecompression = DecompressionMethods.GZip;
    request.Credentials = CredentialCache.DefaultCredentials;

    // Send requests
    int sentRequests = 0;
    Console.WriteLine("Sending HTTP{1} requests to [{0}]:", query, query.Contains("https://") ? "S" : "");

    while (true)
    {
        var response = HttpRequest(request);

        if (response != null)
            DisplayResponse(response, response.ResponseUri);

        sentRequests++;

        if (!Infinite && sentRequests == Requests)
        {
            break;
        }

        Thread.Sleep(Interval);

        if (response != null)
        {
            response.Close();
        }
    }
}
~~~

Стало:
~~~C#
private static void HttpRequestLoop(string query)
{
    ServicePointManager.Expect100Continue = true;
    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);
    request.MaximumAutomaticRedirections = Redirects;
    request.AutomaticDecompression = DecompressionMethods.GZip;
    request.Credentials = CredentialCache.DefaultCredentials;

    int sentRequests = 0;
    Console.WriteLine("Sending HTTP{1} requests to [{0}]:", query, query.Contains("https://") ? "S" : "");

    while (true)
    {
        using (var response = HttpRequest(request))
        {
            if (response != null)
                DisplayResponse(response, response.ResponseUri);

            sentRequests++;

            if (!Infinite && sentRequests == Requests)
            {
                break;
            }

            Thread.Sleep(Interval);
        }
    }
}
~~~
namespace <%= baseName %>.F

module App = 

  open System
  open System.Net

  open Suave
  open Suave.Web
  open Suave.Http
  open Suave.Http.Applicatives
  open Suave.Http.Files
  open Suave.Http.Successful
  open Suave.Types
  open Suave.Session
  open Suave.Log
  open System.IO
  open System.Text

  open System.Data
  open ServiceStack.DataAnnotations
  open ServiceStack.OrmLite
  open ServiceStack.OrmLite.Sqlite

  open Newtonsoft.Json
  open Newtonsoft.Json.Converters

  open <%= baseName %>.F.Domain


  let logger = Loggers.sane_defaults_for Debug

  let dbFactory =
    let dbConnectionFactory = new OrmLiteConnectionFactory("my.db", SqliteDialect.Provider)
    use db = dbConnectionFactory.OpenDbConnection()<% _.each(entities, function (entity) { %>
    db.CreateTable<<%= _.capitalize(entity.name) %>>(false)<% }); %>
    dbConnectionFactory


  type CustomDateTimeConverter() =
    inherit IsoDateTimeConverter()

    do base.DateTimeFormat <- "yyyy-MM-dd"

  let converters : JsonConverter[] = [| CustomDateTimeConverter() |]



  /// Convert the object to a JSON representation inside a byte array (can be made string of)
  let to_json<'a> (o: 'a) =
    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(o, converters))

  /// Transform the byte array representing a JSON object to a .Net object
  let from_json<'a> (bytes:byte []) =
    JsonConvert.DeserializeObject<'a>(Encoding.UTF8.GetString(bytes), converters)


  /// Expose function f through a json call; lets you write like
  ///
  /// let app =
  ///   url "/path" >>= request (map_json some_function);
  ///
  let map_json f (r : Suave.Types.HttpRequest) =
    f (from_json(r.raw_form)) |> to_json |> Successful.ok



  <% _.each(entities, function (entity) { %>
  let <%= entity.name %>Part : WebPart =
    choose [
      GET >>= url "/<%= baseName %>/<%= pluralize(entity.name) %>" >>= request(
        (fun _ -> 
          use db = dbFactory.OpenDbConnection()
          let rows = db.Select<<%= _.capitalize(entity.name) %>>()
          to_json rows |> Successful.ok));

      GET >>= url_scan "/<%= baseName %>/<%= pluralize(entity.name) %>/%d"
        (fun id -> 
          use db = dbFactory.OpenDbConnection()
          let row = db.Single<<%= _.capitalize(entity.name) %>>(fun r -> r.Id = id)
          to_json row |> Successful.ok);

      POST >>= url "/<%= baseName %>/<%= pluralize(entity.name) %>" >>= request(map_json 
        (fun (row : <%= _.capitalize(entity.name) %>) -> 
          use db = dbFactory.OpenDbConnection()
          let num = db.Insert(row)
          row.Id <- int(db.LastInsertId())
          row));

      PUT >>= url_scan "/<%= baseName %>/<%= pluralize(entity.name) %>/%d"
        (fun id -> request(map_json (fun (row : <%= _.capitalize(entity.name) %>) -> 
          row.Id <- id
          use db = dbFactory.OpenDbConnection()
          let old = db.Single<<%= _.capitalize(entity.name) %>>(fun r -> r.Id = id)
          let num = db.Update(row)
          row)));

      DELETE >>= url_scan "/<%= baseName %>/<%= pluralize(entity.name) %>/%d"
        (fun id -> 
          use db = dbFactory.OpenDbConnection()
          let num = db.Delete<<%= _.capitalize(entity.name) %>>(fun r -> r.Id = id)
          Successful.no_content);
    ]<% }); %>



  let localIpAddress : IPAddress = IPAddress.Loopback

  let localPort : Sockets.Port = Sockets.Port.Parse "8080"

  let mimeTypes =     
      Writers.defaultMimeTypesMap
      >=> (function 
          | ".woff" -> Writers.mkMimeType "application/font-woff" false
          | ".woff2"-> Writers.mkMimeType "application/font-woff2" false
          | ".ttf"  -> Writers.mkMimeType "application/font-ttf" false
          | ".eot"  -> Writers.mkMimeType "application/vnd.ms-fontobject" false
          | ".otf"  -> Writers.mkMimeType "application/font-otf" false
          | ".svg"  -> Writers.mkMimeType "image/svg+xml" false
          | _ -> None)
          



  // WEB SERVER

  let config = { 
      bindings            = [ HttpBinding.mk HTTP localIpAddress localPort ]
      serverKey           = toBytes "vC8hd46jydisFLu3oAhD9MprhMTXQQ2WQsbGA5MW"
      errorHandler        = defaultErrorHandler
      listenTimeout       = TimeSpan.FromMilliseconds 2000.
      cancellationToken   = Async.DefaultCancellationToken
      bufferSize          = 2048
      maxOps              = 100
      mimeTypesMap        = mimeTypes
      homeFolder          = Some(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Content"))
      logger              = logger 
      compressedFilesFolder = None
      }
      

  let app = choose [ 
              log logger logFormat >>= never

              // Serve the app.
              GET >>= path "/" >>= browseFileHome "index.html"

              <% _.each(entities, function (entity) { %>
              <%= entity.name %>Part; <% }); %>                

              //serves file if it exists
              GET >>= browseHome 

              // Defaults
              RequestErrors.NOT_FOUND "Found no handlers" 
          ]


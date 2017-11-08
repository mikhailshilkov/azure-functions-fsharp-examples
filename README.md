# Azure Functions in F#

Examples of Azure Functions written in F#. Created for demos at 
[FSharping meetup](https://www.meetup.com/FSharping/events/244137693/).

### Prerequisites

- Azure subscription (their is [a free trial for Function Apps](https://functions.azure.com/try) but
F# support seems not optimal there)

- Node.js version 8.x with NPM

- Editor (I use Visual Studio Code with Ionide plugin)

- Azure Storage Explorer for working with Blobs, Queues and Tables

## 1. Function App Created in the Portal

Go to Azure Portal, click `New` button and search for Function App. Click through the wizard 
to create a new Function App.

Open the app when it's created, add a new function. Pick `Timer` as scenario and F# as language.

Replace the contents of `function.json` and `run.fsx` with files from `1-portal-timer` folder.

Observe the logs to see that the function runs every minute and outputs the message about
the meetup duration.

Explore `function.json` file and its syntax for trigger definition.

Explore the portal functionality around Functions.


## 2. HTTP Function Created in the Portal

Create another function in the same app, this time use `HttpTrigger F#` template. Explore different 
available templates while doing so.

Delete `project.json` and `project.json.lock` files and copy the contents of `function.json` and `run.fsx` 
from `2-portal-http` folder.

Go to `Test` tab on the right, and type `{ "name@": "FSharping" }` in Request body. Click `Run` button
and check that `Hello FSharping` is returned back.

Note `name@` parameter name. That's because we used F# record type to define the request body, and that's
how default serialization works for them. This problem will be solved in example 8.

Explore `function.json` file and its syntax for HTTP trigger and output binding definitions.

Note how function is defined in small-case `run`, and that it's in curried form. This is supported by
Functions, but not required.


## 3. HTTP Function Templating HTML from Blob Storage

Follow the same steps to create yet another F# HTTP function, but use files from `3-portal-http-html`
folder.

In the Blob Storage account that you selected while creating the Function App, add a container called
`html`. Put two html files from the folder into this container.

Now, in the portal, click `Get function URL` button and copy paste it to your browser. Replace parameters
in the URL: `{filename}` with `hello` and `{name}` with `FSharping`. Load the URL in the browser and 
make sure that HTML with `Hello FSharping` is rendered. Replace `hello` with `awesome` and load it again,
so see that HTML has changed.

This works due to an input binding to Blob Storage defined in `function.json`. Explore the syntax to
define HTTP route for the trigger, and templated Blob input binding. See how the function code glues
them together.


## 4. Function Created with CLI

Azure Portal is great to get started, but it's not the most developer-friendly way to write code.
Instead, we will create all the remaining functions locally on dev machine, and deploy it to Azure
from there.

Run the following command to install Azure Functions CLI and runtime on your machine:

``` bash
npm install -g azure-functions-core-tools
```

After it's done, type `func` command to make sure the installation was successful. 

Run `func init` to initialize a new Function App.

Run `func new`, pick `F#` in choice menu, then `HttpTrigger`, then give it a name.

CLI will generate the files required for your new function. Browse through the files to see what
was generated.

Run `func host start` to run your function locally. Runtime will run an HTTP server and will host
your function on port `7071`. 

Run the following command in another command line window (replace the function name if needed):

``` bash
curl http://localhost:7071/api/cli-http?name=fsharping
```

Check that response `200 OK` with body `Hello fsharping` was returned.

Open the Function App folder in Visual Studio Code editor (`code .` command). Notice the multiple
red squigglies while viewing `fsx` file. That's because Functions runtime imports several libraries
"auto-magically" while running, but Code doesn't know about them.

To fix those, copy the following headers to the top of `fsx` (replace `username` with your user):

``` fs
#if INTERACTIVE

open System

#I @"C:/Users/username/AppData/Roaming/npm/node_modules/azure-functions-core-tools/bin/"

#r "Microsoft.Azure.Webjobs.Host.dll"
open Microsoft.Azure.WebJobs.Host

#r "System.Net.Http.Formatting.dll"
#r "System.Web.Http.dll"
#r "System.Net.Http.dll"
#r "Newtonsoft.Json.dll"

#else

#r "System.Net.Http"
#r "Newtonsoft.Json"

#endif
```

Make sure errors are gone from VS Code.


## 5. Precompiled Function

Scripts are nice for dynamic exploration, but I prefer precompiled libraries for more complex
applications. The same applies to Azure Functions: you can deploy normal Class Libraries as 
functions.

Current version of CLI doesn't support creating precompiled functions. It does support running
and deploying them!

For the rest of the demos, I chose to use cross-platform version 2 of Functions runtime, which
is currently in beta. To follow along:

- Go to the portal and create a new Function App. Click on its name, then go to `Function app settings`
and switch the runtime version toggle to `beta`

- Install core version of CLI and runtime by running `npm install -g azure-functions-core-tools@core`

Now, explore the code in `5-precompiled` folder in Code. Note `PrecompiledApp.fsproj` file with
a reference to `Microsoft.AspNetCore.Mvc.Core` and `Microsoft.NET.Sdk.Functions` packages.

`PrecompiledHttp.fs` contains the module with function definition. It's using ASP.NET Core classes.

`function.json` file is still present, but now it contains `scriptFile` and `entryPoint` attributes.

Run the following commands to build, publish and run your Function App locally, all from the root 
folder of this sample:

``` bash
dotnet restore
dotnet build
dotnet publish
func start func start --script-root bin\\debug\\netstandard2.0\\publish
```

Alternatively, just execute `run` command in Visual Studio Code.

Execute `curl http://localhost:7071/api/precompiled` or open this URL in the browser to make sure
that the App is running.

The cool part is that debugging is also supported! Put a breakpoint on `log.Info...` line with F9,
then press F5, choose `dotnet.exe` process to attach to and request the URL again. The breakpoint
should hit.

When you are ready, publish your Function to Azure by running

``` bash
func azure functionapp publish <your-existing-app-name>
```

## 6. Suave Function

What can we do with precompiled functions? Well, we can do a lot, for example use our favourite
F# libraries.

Open the folder `6-suave`. This demo shows the use of [Suave](http://suave.io) library to process 
HTTP requests. 

`function.json` now defines a wildcard route to redirect all requests starting with `/api/suave` to the
function.

`SuaveHttp.fs` has a definition of an app which will look familiar to all Suave users:

``` fs
let app = 
  GET >=> choose
    [ path "/api/suave/hello" >=> OK "Hello GET"
      path "/api/suave/goodbye" >=> OK "Good bye GET" ]
```

The function is then just a one-liner wiring Suave app into the pipeline. 

Run the application and request a URL `http://localhost:7071/api/suave/hello` to see it in action.

This sample is very simple, but you can do lots of powerful stuff with Suave!


## 7. Atrribute-Based Functions

Up until now, we were writing `function.json` files manually for each function. This is not very
tedious, but error prone. There is an alternative programming model where these files are 
auto-generated by Functions SDK.

This programming model is based on attributes, which are similar to WebJobs SDK attributes.
You can find an example of HTTP function with attributes in `7-attributes` folder.

Note that there's no `function.json` file in the project. Instead, the function declaration is
decorated with attributes:

``` fs
[<FunctionName("AttributeBased")>]
let run([<HttpTrigger>] req: HttpRequest, log: TraceWriter)
```

The same development flow still works. Once you run `dotnet build`, a new `function.json` file
will be generated and place to `bin` folder. Function runtime will be able to use it to run
the function as usual.

Make sure that everything still works by executing `run` task in Code.


## 8-9. Demo App

The final demo is a sample application that consists of 5 Azure Functions:

- **`Landing`** is HTTP GET function which returns an HTML page with a form. User can fill this
form to submit their review of the meetup. The review is then posted to `Send` function.

- **`Send`** is HTTP POST function that accepts the review text from user's form and puts it
into `poll-item` queue

- **Twitter** function pulls items from a Storage Queue with twitter-submitted feedback. The
queue is populated by an Azure Logic App which is not part of this repository (there's no code
there, it just listens to a Twitter hashtag). The function then also puts the feedback to
`poll-item` queue

- **Sentiment** function receives messages from `poll-item` queue, then calls Azure Cognitive
Services to get a sentiment score of the message (from 0.0 to 1.0). It then saves the text
and the score into Table Storage

- **Result** function reads all the feedback from Table Storage, calculates an average score
and retrieves all the data as JSON

The inteconnection of these functions is shown on the following chart:

![App Function Graph](/8-app/FunctionGraph.png)

The cool thing about this chart is the fact that it was auto-generated based on `function.json`
files of Azure Functions in `8-app` folder. The tool to generate such chart is in 
`tool-function-graph-gen` folder and is based on the [script](https://gist.github.com/mathias-brandewinder/cdd1e0d23bd3047ffe438d48689b2b86)
from [Mathias Brandewinder](http://brandewinder.com/2017/04/01/azure-function-app-diagram/).
The tool generates a text file in GraphViz format, which can be visualized by multiple tools,
e.g. at [WebGraphviz](http://www.webgraphviz.com).

`9-app` is the same application, but implemented with attribute-based approach instead of
manually created `function.json` files. It's less work to do manually, but it can't be used
for graph generation directly.
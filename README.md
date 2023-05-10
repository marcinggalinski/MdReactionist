# MdReactionist

This is a simple Discord bot written in .NET 7 (C# 11) for the use of myself and my friends on our Discord server. It started as an inside joke, but I decided to make the repo public so that anyone is free to use it or base on it.

It was originally a console app, but was later changed to a Web API. The reason for that was mainly problems I had with Azure, on which it was hosted at first, but it might be ither used in some way or switched back to console app later.

# Requirements

`dotnet-sdk7.0` is required to build project and `aspnet-runtime7.0` is required to run it. You can get instructions on installing them at [.NET download page](https://dotnet.microsoft.com/en-us/download). On some Linux distros (e.g. Arch/Manjaro) they can be installed directly from repo.

# Building

Building instructions are standard for a .NET application. From repository root directory, run `dotnet build src/MdReactionist.sln` to compile project, or `dotnet publish -c Release src/MdReactionist/MdReactionist.csproj` to generate production-ready binaries. You can also use an IDE such as Microsoft Visual Studio or JetBrains Rider.

# Running

Running instructions are standard for a .NET application. In dev environment, from repository root directory you can run `dotnet run src/MdReactionist/MdReactionist.csproj` to run application, or run `dotnet MdReactionist.dll` in publish binaries directory. You can also use an IDE such as Microsoft Visual Studio or JetBrains Rider.

Mark that production environment might require configuring HTTP server (e.g. nginx) to allow connecting to the API from the outside.

# Docker support

It is also possible to run bot in Docker container. To build it, from repository root directory run `docker build -t <image_name> -f Dockerfile .`. Then, run `docker run --env-file=.env -it --rm <image_name>` to start container. Alternatively, create container first using `docker create --env-file=.env --name <container_name> <image_name>` and then start it using `docker start <container_name>`

# Configuration

Bot configuration is handled in two ways. First one, the Discord bot token, is passed via environmental variable `MD_BOT_TOKEN`. If using Docker, you should put token in `.env` file - remember not to push it to remote repository though!

Everything else, i.e. actions configuration, is contained in `BotOptions` object in `appsettings.json` file. It has the following structure:

```json
{
  "BotOptions": {
    "EmoteReactions": [
      {
        "EmoteId": "<emote_id>",
        "TriggeringUserIds": [
          123
        ],
        "TriggeringRoleIds": [
          123
        ],
        "TriggeringSubstrings": [
          "trigger"
        ]
      },
      {
        "Emoji": "\uD83C\uDF46",
        "TriggeringUserIds": [
          123
        ],
        "TriggeringRoleIds": [
          123
        ],
        "TriggeringSubstrings": [
          "trigger"
        ]
      }
    ],
    "Corrections": [
      {
        "StringToCorrect": "incorrect_string1",
        "CorrectedString": "correct_string1"
      },
      {
        "StringToCorrect": "incorrect_string2",
        "CorrectedString": "correct_string2"
      }
    ]
  }
}
```

Both `EmoteReactions` and `Corrections` are arrays of objects, as you can have multiple rules for each action. They will work independently from each other, i.e. each one will be run on every message received by bot, regardless of whether any of the previous ones ended up in any action on the server.

`EmoteReactions` is a set of rules that trigger bot to react with a specified emoji or emote (emoji is a standard Unicode emoticon thingy, while emote is a custom one added to the server), based on user or role mentioned in message, or string contained in message. As for now, each rule can only cause one emote and one emoji reaction - although it can trigger both reacting with emoji and emote.

Emojis can be copied from Discord (or anywhere else, as long as it's supported by Discord) and pasted into the config file. You can get emote id by sending `\:emote_name:` in Discord (i.e. the same as you would normally do to send an emote, but prefixed with a backslash '\\'). In the same manner you can get user id and role id - just mention it as you normally would, but prefix with a backslash '\\', e.g. `\@john.wick`.

`Corrections` is a set of rules that trigger bot to reply with a correction, i.e. `*<corrected_string>` message. Each rule causes up to one reply to be sent. Note, that `StringToCorrect` does not have to be a separate word, but rather a substring in message, e.g. "swimming" will trigger a rule with `StringToCorrect` equal "ing". The bot will replace incorrect part with `CorrectedString`, but will only produce one word, though. For example, for `StringToCorrect` equal "arch" and `CorrectedString` equal "Arch", it will reply:

- "*Arch" to "arch"
- "*Arch-based" to "arch-based"
- "*Architecture" to "architecture"
- "*mArch" to "march"

# Credits

Reason for creating - Michał Drętkiewicz

Development - Marcin Galiński

Testing and idea providing - Michał Drętkiewicz, Piotr Duperas (@piotrduperas), Maciej Chotkowski

# License

The MIT License (MIT)

Copyright (c) 2023 Marcin Galiński

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

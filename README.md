# Welcome to the .NET Foundation Web Site!

This website powers <http://dotnetfoundation.org>.

## How to build this site

### Prerequisites

* You'll need something that can build .NET Core projects
* You'll need an IDE of some kind (Visual Studio or VSCode works well)
* You'll need SQL Server with LocalDB installed so that the CloudScribe CMS will automatically set itself up.

### Getting the site code

* Fork this repository.
* Clone your forked repository onto your machine.

### Opening the Project

Open the project in the editor of your choice

#### How to Build

* If you're in Visual Studio, build as you would a normal project.
* If you're on the command line, run `dotnet restore` and then `dotnet build`

#### How to run

* You should be able to run the site with `dotnet run` or the classic `F5` or run commands within Visual Studio.

NOTE: When running, the site will attempt to set up the localDB automatically, assuming you have a functioning installation of LocalDB.

## Any Trouble?

Let us know if you run into issues and we'll do our best to get you up and running.
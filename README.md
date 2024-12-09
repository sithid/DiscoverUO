# DiscoverUO
- Ultima Online Public Server List
- A Blazor WebApp © 2024, James Glosser
- demonicurges05@gmail.com 

## Purpose
DiscoverUO is a place where users can discover and share Ultima Online servers that are administered and maintained by players just like themselves.

## Dedication
On 10/10/2024 my best friend, Ben Barnett, passed away in a tragic car accident.  While I had already started the process of planning this project, it is dedicated to him. Ben was that one friend who got me into everything, including Ultima Online.  When I first started to plan to do this project, Ben is someone I was excited to get ideas from.  I feel it is only fitting that I dedicate this project to him.

## Planned Features
This is a list of planned features.  It is my intention to put my best effort into including all of the features on this list, however, some features may not make it.  A big part of these features assumes I include a user login/register system.

- Register/Login: Users have an option to register an account and login.  Users will have profiles which can, by choice, show all servers that the user has added to the public server list.
- View List:  Look through and explore a list of publicly available player-run servers.
- Add Server: As a registered user, the user can add servers to the public server list.
- Remove Server:  As a registered user, the user can remove any public server the user has added to the list.
- Favorites: As a registered user, create and manage a list of your favorite servers.
- [TODO] Vote:  Registered users can vote on their favorite public servers within the public server listing.  Votes will serve as a ranking system for public servers on the server list. When browsing the public server list, users will have the ability to sort by various criteria included but not limited to: Era, Votes, Sponsored (servers added by admin). User voting should be limited to 1 vote per day.


## Technical
- Blazor WebApp: Frontend WebApp
- WebApi: Perform all CRUD operations.
- Class Library: Shared entity models / DTOs.
- Database: SqLite database with tables for users, servers,  reviews, server blacklist, user blacklist, and favorites.

## User Access Privileges
Anonymous / Unregistered: Anon can access the WebApp and view the current public server listing. Anon may not add to, remove from, or otherwise modify the server listing in any way.

Registered User: A registered user should be able to browse the public server listing.  Users should also be able to add a server to the public server list for the public aswell as maintain a list of their favorite servers.

Elevated User: A registered user with elevated privileges, ie a moderator and administrator.  Moderators can ban users as well as remove servers. Admins can do anything a moderator can do, as well as ban a moderator.

Banning not yet implemented, planed for a future update.

## Requirements (grouped together where convient)

```
Create and utilize a minimum of 3 functions or methods, with at least one
returning a value integral to your application.

Develop at least one class (excluding the default class in a new project),
create an object of that class, populate it with data from a database, and incorporate the data in
your application. A minimum of 1 table (entity) should be utilized. Note that classes should be
created even when using object-relational mappers like Entity Framework.

Make a generic class and use it.
```
- `DataManager.cs` from `DiscoverUO.Web.Components.Data` and `Utilities.cs` from `DiscoverUO.Api` for specific functions that would be considered integral to the application.
- Review controllers for additional methods/functions including endpoints and a couple helper methods.
- Review the entire `DiscoverUO.Lib.Shared` namespace for DTO models (classes whos name ends in `Data`).
- Review `DiscoverUO.Api.Models` for entity models.
- `MappingProfile.cs` contains Maps for model to dto conversion.
- Generic classes were used for how I handle my endpoint return data / responses.  Review `DiscoverUO.Lib.Shared.Contracts` as well as various classes which inherit from `IEntityResponse`, `IListResponse`, and `IValueResponse`.


```
Make your application an API. 
Make your application a CRUD API
Have 2 or more tables (entities) in your application that are related and have a function return data from both entities.  In entity framework, this is equivalent to a join
```
- My API implements endpoints for all essential CRUD (Create, Read, Update, Delete) operations for 1 database (`DiscoverUO.db`) across 3 controllers on 5 tables.
- My data models include:
  - Users
  - UserProfiles
  - UserFavoritesLists
  - UserFavoritesListItems
  - Servers
- Among my entity models, there are various relationships. All users have a profile and a favorites list. All favorites lists have 1 owner and it can have man favorite items. All servers have one owner ( typicall the person who added it ) but can be on many favorites lists.  My dashboard endpoint ('https://localhost:7015/api/users/view/dashboard', 'GetDashboardData') is a good example of returning data from related entities.

```
Implement a regular expression (regex) to validate or ensure a field is always stored and displayed in the correct format .
Create a list, populate it with several values, retrieve at least one value, and use it in your program.
```


Examples of both of these can be seen in how I validate user registration data.  I use regex to verify password length and complexity aswell as ensuring there is an @ in email strings.  Domains can very so I am not entirely sure how I would validate an email beyond that. I created and populate a list with error messages and then display them to the user when they are attempting to register.

```
Solid Principles
```


I believe the way I have implemented endpoint responses implements SOLID principles. My understanding of the principles of SOLID and recognizing them when they are implemented gives me trouble.
This is something I would welcome feedback on (such as where in my project I have implemented SOLID properly, the specific principle and how it applies, so i can better recognize it in my design patterns).
 

```
Visual Appeal
```


Sadly, I am terrible with visual design.  I have always considered myself to be a "backend" kind of guy and value function over fashion. I intend to see this project further than just where the Code:You capstone takes it, so the visual appeal will be worked on heavily in a future update.

## Setup and Running Project

Clone the repo and launch the visual studio solution. Everything should build out of the box. Make sure when you build and launch, your startup project is set to multiple projects including DiscoverUO.Api and DiscoverUO.Web.Database should include sample data.  For some of you, you may have to navigate certain SSL issues, depending on your browser settings. 
![image](https://github.com/user-attachments/assets/a75e67ce-bf0e-4459-a077-db959b6aaf56)

Please trust / install cert where applicable.

![image](https://github.com/user-attachments/assets/c2082689-49fc-4396-b43c-94e788106736)
![image](https://github.com/user-attachments/assets/0725330c-bd33-4fda-917d-59c1d36b21da)

Use a test admin account to access all crud options.

If you build and run in debug, two browser windows will load.

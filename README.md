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
- Database: SQLServer database with tables for users, servers,  reviews, server blacklist, user blacklist, and favorites.

## User Access Privileges
Anonymous / Unregistered: Anon can access the WebApp and view the current public server listing. Anon may not add to, remove from, or otherwise modify the server listing in any way.

Registered User: A registered user should be able to browse the public server listing.  Users should also be able to add a server to the public server list for the public aswell as maintain a list of their favorite servers.

Elevated User: A registered user with elevated privileges, ie a moderator and administrator.  Moderators can ban users as well as remove servers. Admins can do anything a moderator can do, as well as ban a moderator.

Banning not yet implemented, planed for a future update.

## Setup and Running Project

Clone the repo and launch the visual studio solution. Everything should build out of the box. Make sure when you build and launch, your startup project is set to multiple projects including DiscoverUO.Api and DiscoverUO.Web.Database should include sample data.  For some of you, you may have to navigate certain SSL issues, depending on your browser settings. 
![image](https://github.com/user-attachments/assets/a75e67ce-bf0e-4459-a077-db959b6aaf56)

Please trust / install cert where applicable.

![image](https://github.com/user-attachments/assets/c2082689-49fc-4396-b43c-94e788106736)
![image](https://github.com/user-attachments/assets/0725330c-bd33-4fda-917d-59c1d36b21da)

Use a test admin account to access all crud options.

If you build and run in debug, two browser windows will load.

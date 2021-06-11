# BooksClient
So this is it, huh? The client. It's my first WPF application, my first .NET application and an overall mess. It doesn't work without some obscure DLLs I found in forum posts from 2006. This is what we in Germany would call a bucket containing professionals ("Eimer mit Profis").

## What
The BooksClient is, at least nominally, capable of displaying and editing selected bibliographical metadata for a small amount of books stored in a database connected to the REST API available [here](https://github.com/antifuse/booksserver). It will also import data from Z39.50 databases (currently configured to use the British National Bibliography at the British Library, authentication required) and SRU interfaces (currently configured to use the German National Bibliography at the DNB, no authentication required).

## How
The program will crash initially. This is expected behaviour. Edit the settings file to add a working API base URL to enable the client to get the full list of stored books from the specified server. 

## Why
I just felt like it. Together with the back-end and working out how the hell MARC21 works, this was one month's irregular evening work. There is zero love in this project. At least I hope so.

**This project is highly unstable and will crash when it feels like it. If you want to change that, feel free to. I know you won't.**

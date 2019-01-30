# DuplicateFinder #
A WPF Application for easily finding and removing duplicate files. This was a utility built to help serve a problem I have with my own catalog of family pictures, it's hard to know if I have a copy of a photo somewhere else in my archive.
This was also my first time leveraging the full strengths of WPF with async C#, and I wanted to also learn what MVVM is and how it differs from MVC.

## Description ##
Allows a user to scan files locally and on mapped network drives and stores the SHA256 hash value of these files to a local db.

## Requirements ##
 * SQL Server Express localdb installed

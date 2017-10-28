PS6 SPREADSHEET
Program created by Bryan Hatasaka and Marcus Hahne for the PS6 assignment, CS3500-001, 
October 27, 2017. Both Bryan and Marcus confirm that this assignment was written by them alone, 
without copying any outside code.

--SUMMARY OF ADDITIONAL FEATURES--
We put the WriteCellContents method into a BackgroundWorker, so that the GUI does not freeze while
performing large functions. Spreadsheets with many dozens of dependencies will remain functional as
they're recalculated. We chose to add a BackgroundWorker as a demonstration of the skills we learned
in class.

Additionally, when the program is installed as an actual application in Windows, 
.sprd files are set to open with the spreadsheet program and will load up like a mainstream program would
do.

As a sort of joke, we also added the ability to put a racing stripe on the spreadsheet, to make it
look super cool. The use of a racing stripe is optional and not recommended for total squares.

A save as button was added that will allow the user to change the name of the spreadsheet, even after they
have altered it.

--SPECIAL INSTRUCTIONS--
Though PS6 does not have all the capabilities of a more mainstream spreadsheet application, it does
work in the same way. When data is entered into a cell, hitting "Enter" on the keyboard puts the data
in the cell and displays the value in the spreadsheetPanel. 

--IMPORTANT DESIGN DECISIONS--
We did not use the overloaded Spreadsheet constructor to load saved data from a file. Instead, the
Load button itself reads the data and fills out the spreadsheet. In hindsight, this was probably a
mistake, because the Spreadsheet constructor has that functionality already, so this was an anti-
pattern. We'll be sure not to make that mistake again in the future.

One design feature we decided to include was in the "Save" button, where the program differentiates
between "Save" and "Save As" when it is clicked. Clicking "Save" when the spreadsheet has not been
loaded from a file calls the "Save As" function, or "Save" otherwise.
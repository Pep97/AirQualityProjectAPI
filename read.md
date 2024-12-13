Overview
Welcome to the PersonalProject! This program allows you to check the air quality in major cities around the world and download the data as a CSV file. The program interacts with the AirVisual API to fetch real-time air quality data.

Features

•	Search for air quality data by country, state, and city.
•	Display air quality data including pollution and weather information.
•	Download air quality data as a CSV file.

Prerequisites

•	.NET 8 SDK
•	An API key from AirVisual. You can get one by signing up at AirVisual.

Usage

1.	When you run the program, you will be greeted with a welcome message.
2.	You will be prompted to choose an option:
•	[1] -> Look for air quality in your city
•	[2] -> Exit
3.	If you choose option 1, you will be asked to enter the country where the city is located.
4.	After entering the country, you will be shown a list of available states in that country.
5.	Enter the state where the city is located.
6.	You will then be shown a list of available cities in that state.
7.	Enter the city you want to check the air quality for.
8.	The program will display the air quality data for the selected city.
9.	You will be given options to:
•	Download the city data as a CSV file.
•	Search for another city.
•	Go back to the main menu.

Error Handling
•	If the API rate limit is exceeded, you will see a message indicating that you should try again later.
•	If the country, state, or city is not available, you will be prompted to search again or go back to the main menu.
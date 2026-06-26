# SecureWin Cybersecurity Awareness Chatbot

## Student Information

**Student Name:** Erwin Mashobane
**Student Number:** ST10073464

# Project Description

SecureWin is an intelligent Cybersecurity Awareness Chatbot developed in C# using WPF and .NET 8.0.

The application was developed in three phases:

* **Part 1:** Console-based cybersecurity chatbot with keyword recognition, sentiment detection, and memory features.
* **Part 2:** Graphical User Interface (GUI) implementation using WPF with improved user interaction.
* **Part 3:** Advanced chatbot functionality including task management, reminders, activity logging, JSON persistence, Natural Language Processing (NLP), quizzes, and enhanced user experience.

The chatbot educates users about cybersecurity best practices while allowing them to manage personal cybersecurity tasks.

# Features

## Part 1 Features

* User greeting with personalised interaction.
* Username memory.
* Keyword recognition for cybersecurity topics.
* Sentiment detection.
* Cybersecurity awareness responses.
* Conversation history.
* Favourite topic memory.
* Random follow-up responses.
* Console-based chatbot interaction.

## Part 2 Features

* Modern WPF graphical user interface.
* Chat-style messaging interface.
* User and chatbot message bubbles.
* Automatic scrolling.
* Keyboard support (Enter key).
* Improved navigation.
* Cybersecurity tips.
* Enhanced chatbot interaction.

## Part 3 Features

### Task Management

* Add tasks.
* View tasks.
* Delete tasks.
* Complete tasks.
* Task reminders.
* JSON task persistence.

### JSON Data Storage

* Tasks are automatically stored in:

Data/tasks.json

### Quiz System

* Interactive cybersecurity quiz.
* Score tracking.
* Retry quiz option.
* Retry full quiz option.
* Full-screen results display.
* Keyboard navigation support.
* Activity logging for quiz actions.

### Activity Logging

The chatbot records:

* Task creation.
* Task deletion.
* Task completion.
* Reminder creation.
* Quiz start.
* Quiz completion.
* Chat interactions.
* NLP actions.
* Window navigation.

Users can ask:

Show activity log

or

What have you done for me?

to view recent activity.

### Natural Language Processing (NLP)

The chatbot recognises commands such as:

Add task
View tasks
Delete task
Complete task
Quiz
Show activity log
Leave session

### Memory Features

* Remembers returning users.
* Stores conversation history.
* Displays previous chats when users return.

### Additional Features

* Session management.
* Leave session command.
* Dynamic chatbot responses.
* Automatic focus handling.
* Mouse wheel scrolling support.
* Keyboard accessibility support.

# Technologies Used

* C#
* WPF
* .NET 8.0
* JSON
* Visual Studio 2022
* Newtonsoft.Json


# Prerequisites

Before running the application, ensure the following are installed:

* Visual Studio 2022
* .NET 8.0 SDK
* Newtonsoft.Json NuGet Package

---

# Installing Newtonsoft.Json

1. Open the project in Visual Studio 2022.
2. Right-click the project.
3. Select:

Manage NuGet Packages

4. Click:

Browse

5. Search for:

Newtonsoft.Json

6. Click:

Install


# Project Setup Instructions

## Step 1

Clone the repository:

git clone <repository-url>


## Step 2

Open the solution file:

CybersecurityAwarenessChatbot.sln

using Visual Studio 2022.


## Step 3

Restore NuGet packages.

Visual Studio normally restores packages automatically.

## Step 4

Build the solution:

Build → Build Solution

## Step 5

Run the application:

Debug → Start Debugging

or press:

F5


# Greeting Audio Setup

Place the file:

greeting.wav

inside:

Audio/

Example:

CybersecurityAwarenessChatbot
│
├── Audio
│   └── hello.wav

The application will automatically play the greeting when the chatbot starts.

# JSON Setup

No manual setup is required.

The application automatically creates:

Data/tasks.json

when the first task is added.


# Screenshots

## Running GUI
![Running GUI](Images/RunningGUI1.png)
![Running GUI](Images/RunningGUI2.png)

## GitHub Actions Green Tick
![GitHub Actions] (Images/GitHub_actions.png)

# YouTube Demonstration

Unlisted YouTube demonstration:

https://youtu.be/k0wUWVrz0ZQ

## Releases

### v3.0 – Task Assistant and JSON Storage Integration
Created after the Task Assistant GUI and JSON persistence were fully implemented and tested.

### v3.1 – Quiz and Activity Log Features Added
Created after integrating the Cybersecurity Quiz and Activity Logging functionality.

### v3.2 – Final Integrated Version
Final version containing full integration of Parts 1, 2, and 3, ready for submission.

# Author

**Erwin Mashobane**
**Student Number: ST10073464**

# License

This project was developed for academic purposes as part of the Independent Institute of Education (IIE) coursework.

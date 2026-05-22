# MyPersonalDjGui 

A modular, multi-tiered desktop audio playback application built with **C#** and **WPF (Windows Presentation Foundation)**. This project represents the evolution of a legacy console-based media application into a modern graphical user interface (GUI) leveraging event-driven programming, asynchronous processing, and a clean separation of concerns.


##  Features

* **Decoupled Multi-Tier Architecture:** Complete separation between the UI presentation layer, data data models, and core hardware playback components.
* **Dynamic Directory Scanning:** Automatically scrapes local file directories to extract, parse, and catalog media assets (`.mp3` format) into active UI list controls.
* **Real-time Hardware Interface:** Utilizes external media foundation libraries to interface directly with host sound systems for low-latency audio state transitions (`Play`, `Pause`, `Stop`).
* **Sleek Custom UI Layout:** Implements a dark-themed control surface optimized for readability, complete with localized control mapping and dynamic data container streaming.


##  System Architecture

The codebase utilizes an optimized structural layout inside the `src` directory to maintain modularity and ease future cloud migrations:

```text
MyPersonalDjGui/
│
├── App.xaml / App.xaml.cs          # Application Bootstrap & Main Thread Entry
├── MainWindow.xaml                 # Front-End Visual Interface (XAML Layout)
├── MainWindow.xaml.cs              # Code-Behind (UI Event Trigger Controls)
│
└── src/                            # Business & System Core Logic
    ├── playlist.cs                 # Structured Media Track Data Model
    ├── songmenu.cs                 # Playlist Collection & Aggregation Engine
    ├── playback.cs                 # Audio Driver Control Pipeline
    └── audioplayer.cs              # Core Hardware Interface Class
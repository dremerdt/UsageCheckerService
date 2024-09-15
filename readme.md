## CPU & Memory Monitoring Service

A simple service that monitors CPU and memory usage, sending email notifications when predefined thresholds are reached. 

It leverages the following:
- Mailgun for sending email alerts.
- PerformanceCounter for reading real-time CPU and RAM usage data.
- BackgroundWorker with PeriodicTimer to run recurring checks at defined intervals.


This lightweight solution is ideal for system resource monitoring and proactive notifications based on performance thresholds.


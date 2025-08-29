# Projekt grupowy

This is our group project – a web application built with ASP.NET.

## Hosting and Deployment

The application is currently hosted on **AWS**:

- **EC2 (Amazon Linux)** instance serves as the web server.
- The application is **dockerized** and stored in **Amazon ECR** for container management.
- **MSSQL Database** is hosted in **RDS** (Relational Database Service).
- The Docker container is deployed and runs on the EC2 instance, exposing the web application to the public.
- http://13.217.97.150/

## Description

The application includes the following features:

- Integration with the **Ticketmaster API** to fetch event data.
- **Pagination** and **event search** via query or form.
- **User registration and login** using **OAuth** and **ASP.NET Identity**.
- After selecting an event, the user is redirected to the **Stripe payment** section.
- Upon successful payment:
  - A **QR code** is generated containing the URL of the selected event.
  - A **PDF file** with event details is created using QuestPDF.
  - An **email with the PDF** is sent via **SMTP**.
  - An **SMS with the event URL** is sent using **Twilio**.
- Application uses **logging** for error tracking and diagnostics.
- Database operations are handled using **Entity Framework**.
- The project uses **Scaffolding** for rapid development of UI components.

## Technologies Used

- **ASP.NET MVC**
- **Ticketmaster API**
- **Scaffolding**
- **Payments** – Stripe
- **OAuth** – External login providers
- **SMTP** – For sending emails
- **QR Code Generator** – QRCoder
- **PDF Generator** – QuestPDF
- **SMS API** – Twilio
- **Logger**
- **Entity Framework**

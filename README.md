POE Part 3 YouTube 
https://youtu.be/4ARdkufoyR0

## How to Run the Application

This solution contains two projects: the **API** and the **MVC Front-End**. Because the MVC application relies entirely on the API for data and authentication, **both projects must be running at the same time** for the application to work correctly.

Please follow these steps to run it locally in Visual Studio:

### Step 1: Start the API (Backend)
1. In the Solution Explorer, right-click the **`RaceDay`** (API) project.
2. Select **Debug** > **Start New Instance**.
3. A browser will open with Swagger. Ensure the URL is running on `http://localhost:5111`. Leave this window running.

### Step 2: Start the MVC Application (Frontend)
1. Go back to Visual Studio.
2. Right-click the **`RaceDayMVC`** project.
3. Select **Debug** > **Start New Instance**.
4. A new browser window will open with the RaceDay website. You can now register, log in, and use the application.

*(Note: Alternatively, you can run the MVC app inside a Docker container using the instructions below, but the API must still be running locally in Visual Studio on port 5111).*

---

##  Docker Instructions (For the MVC App)
To run the MVC application inside a Docker container, execute the following commands from the root folder of the solution:

1. Build the Docker image:
   `docker build -t raceday-mvc -f RaceDayMVC/Dockerfile .`

2. Run the container:
   `docker run -d -p 8080:8080 --name raceday-container raceday-mvc`

The MVC application will be accessible at: `http://localhost:8080`

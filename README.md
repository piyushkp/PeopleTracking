
### People Tracking System - Image Processing

I have used OpenCV and EmguCV in this project. This project is having following Image processing features implemented: 
#####1. Face Detection (Frontal and side) - 85-90 % accuracy rate  
#####2. Face Recognition - 85 % accuracy rate  
#####3. Age detection - 80 % accuracy rate  
#####4. Gender detection - 90-95 % accuracy rate  



At a high-level following are the key features are developed in this application:

1)  In-Store Circulation: Track people moving in a store.   
2)	Window impressions : How many shoppers crossing the store window turn and notice the window communication  
3)	Hot-Spots: Track specifics spots in-store which witnesses maximum shopper density.  
4)	Store Layout: Generate a store layout using the feeds.  
5)	Shopper Journey: Segregate Men, Woman and kids amongst the people moving in a store.  

Requirement# 1: In-store Circulation Track people moving in a store. 

Using the CCTV Camera feeds it should be possible to generate a circulation map of the store. 
The circulation map essentially tracks the paths taken by people while they are in-store 
or while they are walking around in the store. It should be possible to measure the following:  
1)	How people are moving in a store, moving path is required.   
2)	How people move by day, by periods of the day.  
3)	Men, woman and kid segregation is required.  
4)	Aggregate movement and also break-up as per above point.  

Requirement# 2: Window Efficacy measure (window impressions): Track how many people are stopping in front of a Camera.

This shows the number of people (count) who are turning to look at the store while walking past and 
then specifically stopping in front of a display window. This may result into the total number of people 
who visited the store. Hot spots can be created the moment we have someone looking at window with full frontal face. 
We also need to identify full face vs. side facing, this will give very good idea on how many people are looking at window and help analyse window display efficacy.  

Requirement# 3: Hot-Spots: Track where people are spending more time within a store and 
    where they are spending less time.



Thanks,

#####Piyush Patel  
#####er.piyushpatel@gmail.com

# DeMaio Thesis Results

This folder contains results from the experiments presented in DeMaio's thesis. For each experiment, an Excel spreadsheet containing the results is given, along with a folder containing all the pictures taken for the experiment. I also provide the MATLAB script used to visualize the data.
All pictures were analyzed using the [Continuum Robot Image Analyzer](https://github.com/comet-lab/Continuum-Image-Analyzer). Ask Nick Pacheco for instructions on how to use this software. For all timestamps in all experiments, two pictures of the tube were taken. 
Each picture was analyzed 5 times for a total of 10 measurements per timestamp per tube. The average of the 10 data points was used as the final measurement.

The experiments are briefly explained below. For a more detailed explanation, refer to DeMaio's thesis.

## Experiment 1

### Purpose
1. Determine repeatability of the manufacturing process described in [Lu 2023](https://ieeexplore.ieee.org/abstract/document/10130226).
2. Determine whether (and, if so, how) tube curvatures change in the hours and days after manufacturing.

### Procedure
1. Manufacture 15 tubes (5 each of 3 different sizes) as proposed in [Lu 2023](https://ieeexplore.ieee.org/abstract/document/10130226) and described in DeMaio's thesis.
2. Measure their curvature at t={0,0.5,1,1.5,2,4,24,48} hours after they are removed from the mold.

### Results
1. The manufacturing process is not reliable.
2. The tubes undergo significant loss of curvature in the time after being removed from the mold, with the most significant relaxation happening within the first hour.

## Experiment 2

### Purpose
Determine accuracy of the elastic model for predicting the equilibrium curvature of a set of plastic tubes.

### Procedure
1. Manufacture a curved tube (OD=2.64mm).
2. Measure the tube's curvature.
3. Insert a straight tube (OD=1.98mm) into the curved tube.
4. Mesaure the nested tubes' equilibrium curvature at t={0,1,2,3,4,5,10,15,20,25,30} minutes after nesting.
5. Remove the straight tube from inside the curved tube.
6. Re-measure the curved tube's curvature.

### Results
1. The equilibrium curvature changes over time, riughly following the shape of a decaying exponential.
2. The elastic model does not accurately predict the equilibrium curvature of plastic tubes.

## Experiment 3

### Purpose
Determine whether Nylon-12 tubes behave elastically in response to loading.

### Procedure
1. Repeatedly straighten a curved tube by pulling it into a straight steel tube and pushing it back out.
2. Take pictures after {0,5,10,15,20,25,30} straightening cycles.
3. Take a picture after the tube has been allowed to rest for 3 days.
4. Repeat for 5 tubes in each of 3 sizes.

### Results
1. Tubes' curvatures decreased as straightening cycles increased (indicating non-elastic behavior).
2. Tubes regained most-all of the curvature they lost in the ensuing 3 days (indicating non-plastic behavior).
3. The deformation is not plastic or elastic, but rather viscoelastic.

## Experiment 4

### Purpose 
Determine if a viscoelastic model can describe the behavior of plastic tubes in response to unloading.

### Procedure
1. Insert curved plastic tube into straight steel tube.
2. Wait 60 seconds.
3. Remove plastic tube from steel tube.
4. Measure plastic tube's curvature at t={0,1,2,3,4,5,10} min after removal from steel tube.

### Results
Viscoelastic model proposed by [Fancey 2001](https://www.degruyter.com/document/doi/10.1515/POLYENG.2001.21.6.489/html) can be used to track the behavior of plastic tubes.

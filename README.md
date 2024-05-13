# CTR-Tube-Properties

This repository contains work completed by Chris DeMaio (MS Spring 24) throughout the 2023-24 academic year. Initially, the goal of this work was to use a Kalman filter to track the precurvatures of a CTR's tubes as the tubes undergo plastic deformation during CTR operation. However, we were unable to track the precurvatures of Nylon-12 tubes, despite much experimentation. We turned our focus to the mechanical properties of the tubes themselves. Through a series of experiments described in Chris's thesis (see demaio_thesis/DeMaio_Thesis_final.pdf), we concluded that Nylon-12 tubes are viscoelastic rather than elastic. As such, the underlying assumptions of the elastic model described in [Webster 2009](https://ieeexplore.ieee.org/document/4671090/) are invalid, and the model fails to describe the behavior of Nylon-12 tubes. We revealed the need for additional work in the modeling of polymer CTRs. 

This repository is organized as follows:

**_code_cs_** contains the C# code used to perform the Kalman filter experiments. It includes a C# implementation of CTR kinematics, encoder interfacing, the Extended Kalman Filter, the Unscented Kalman Filter, and motion capture interfacing.

**_code_ino_** contains the Arduino code used to communicate with the encoders via an Arduino Mega board.

**_data_** contains the pictures taken for the experiments in DeMaio's thesis, Excel spreadsheets showing the results in tabular form, and MATLAB scripts used to visualize the data.

**_mocap_** contains things you will need to download if you want to use the Polaris Vega motion capture sensor, as well as a Getting Started guide.

**_docs_** contains some how-to guides.

**_demaio_thesis_** contains materials from Chris DeMaio's thesis.

Nick Pacheco, Wenpeng Wang, and Prof. FIchera all have a general understanding of everything in this repository and should be able to answer basic questions. If they are unable to answer your questions, please feel free to contact Chris:

christopherdemaio1@gmail.com

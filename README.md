# Color-Based Image Similarity

A Windows Forms application that finds similar images based on color histogram analysis. This project was developed as part of an algorithms course contest, where it achieved **9th place out of 50 participants**.

## Overview

This application uses color histogram analysis to measure image similarity. It calculates statistical features from RGB color channels and uses cosine distance to find the most similar images in a dataset.

## Features

- **Color Histogram Analysis**: Calculates comprehensive statistics for each RGB channel:
  - Histogram (256 bins for each channel)
  - Minimum, Maximum, Median values
  - Mean and Standard Deviation
  
- **Image Similarity Matching**: Uses cosine distance between probability distributions of color histograms to find similar images

- **Parallel Processing**: Leverages multi-threading to efficiently process multiple images simultaneously

- **Interactive GUI**: 
  - Load a folder of target images
  - Select a query image
  - Find top N most similar images
  - Visualize RGB histograms for both query and matched images

- **Performance Metrics**: Displays loading and matching times

## Algorithm

The similarity matching algorithm works as follows:

1. **Image Statistics Calculation**: For each image, compute:
   - RGB histograms (256 bins per channel)
   - Statistical measures: min, max, median, mean, standard deviation

2. **Similarity Scoring**: For each target image, calculate cosine distance:
   - Convert histograms to probability distributions
   - Compute cosine distance for each RGB channel separately
   - Average the three channel distances to get the final similarity score

3. **Top Matches**: Sort all images by similarity score and return the top N matches

The cosine distance formula used:
```
distance = arccos(dot_product / (norm1 * norm2)) * (180 / π)
```

Where the final score is the average of R, G, and B channel distances.

## Requirements

- .NET Framework 4.8
- Windows OS
- Visual Studio (for building from source)

## Building the Project

1. Open `ImageSimilarity.sln` in Visual Studio
2. Build the solution (F6 or Build → Build Solution)
3. Run the application (F5)

## Usage

1. **Load Images**: Click "Load Images" and select a folder containing images
   - Supported formats: JPG, JPEG, PNG, BMP, GIF, TIFF, ICO
   - The application will calculate statistics for all images in parallel

2. **Select Query Image**: Click "Open" to select the image you want to find matches for
   - The query image and its RGB histograms will be displayed

3. **Find Matches**: 
   - Set the number of top matches desired (using the numeric up/down control)
   - Click "Match Image" to find the most similar images
   - Results will be displayed in the matched images list

4. **View Results**: 
   - Click on any matched image to view it and its histograms
   - The match score (lower = more similar) is displayed

## Project Structure

- `ImageHistSimilarity.cs`: Core algorithm for calculating image statistics and finding matches
- `ImageOperations.cs`: Image I/O operations and histogram visualization
- `MainForm.cs`: GUI implementation and user interaction handling
- `Program.cs`: Application entry point

## Performance Optimizations

- **Parallel Image Loading**: Uses `Parallel.For` to process multiple images concurrently
- **Unsafe Code**: Direct memory access for efficient image pixel reading
- **Single-Pass Statistics**: Calculates all statistics in a single iteration through the image

## Contest Achievement

This project was submitted as part of an algorithms course contest and achieved **9th place out of 50 participants**, demonstrating effective implementation of color-based image similarity algorithms.

## License

This project was created for educational purposes as part of an algorithms course.


# MiniPhotoshop

A simple image editing application built with WPF and C#.

## Features

- **File Operations**: Open and save images (JPG, PNG, BMP, GIF)
- **Pixel Data Export**: Save pixel data in 3D array format
- **Color Filters**: Red only, Green only, Blue only, Grayscale
- **Histogram Visualization**: View RGB and Grayscale histograms
- **Contrast Enhancement**: Linear contrast, gamma correction, adaptive contrast, and global contrast stretching
- **Image Smoothing**: Mean, Gaussian, and Median filters
- **Image Sharpening**: Laplacian sharpening
- **Color Spotlight**: Interactive color selection with tolerance slider
- **Image Security**: LSB steganography (embed/extract) and watermarking (text/logo)

### Advanced Image Processing (5 Tugas)

1. **Negasi Citra** - Invert colors using formula: 255 - pixel_value
2. **Brightness Adjustment** - Dynamic slider (-255 to +255) with lossless data handling
3. **Grayscale Comparison** - Side-by-side comparison of Average vs Luminance methods
4. **Interactive Color Selection** - Click to isolate specific colors
5. **Binary Threshold + Negation** - Threshold at 128 then apply negation

*Untuk detail implementasi, lihat [IMPLEMENTASI_5_TUGAS.md](IMPLEMENTASI_5_TUGAS.md)*

## Requirements

- .NET 8.0 or later
- Windows OS

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/kaisabiyyi/miniphotoshop.git
   cd miniphotoshop
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

## Usage

1. Launch the application
2. Click "Pilih Gambar" to open an image file
3. Use the toolbar to apply various editing operations
4. Save your work using the save functionality

## Contributing

Contributions are welcome. Please create a pull request with your changes.

## License

This project is open source and available under the MIT License.

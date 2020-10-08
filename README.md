# DahuaPictureOverlay
Converts an image into a 256 color bmp with a max resolution of 128x128 pixels and max size of 16 KiB, as required for Dahua cameras with the Picture Overlay feature.

## Usage
Download from [the releases tab](https://github.com/bp2008/DahuaPictureOverlay/releases) and drag and drop an image file onto the executable.  A file `out.bmp` will be written in the directory you loaded the image from.  If you want to run the program from the command line, provide the path to the input image as the first (and only) argument.

Supported input formats include BMP, PNG, and JPG.

The input image will be scaled to fit within the limits:
* 128x128 pixels
* 16 KiB file size
* 256 colors

An optimal color palette is computed to suit your image.

Because of BMP file headers and the color table, 128x128 pixels is not actually possible within 16 KiB without compression. Compression is not properly supported by Dahua cameras, so square or near-square images will be scaled a little smaller. E.g. 123x123 pixels.

## Known Issues
It appears that Dahua cameras will arbitrarily choose one of the 256 colors in the bitmap file to be fully transparent.  This may cause your image to appear incorrectly once uploaded as the picture overlay, and it is not the fault of this conversion application.

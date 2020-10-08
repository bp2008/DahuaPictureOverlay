# DahuaPictureOverlay
Converts an image into a 256 color bmp with a max resolution of 128x128 pixels and max size of 16 KiB, as required for Dahua cameras with the Picture Overlay feature.

## Usage
Download from [the releases tab](https://github.com/bp2008/DahuaPictureOverlay/releases) and drag and drop an image file onto the executable.  A file `out.bmp` will be written in the directory you loaded the image from.

## Known Issues
It appears that Dahua cameras will arbitrarily choose one of the 256 colors in the bitmap file to be fully transparent.  This may cause your image to appear incorrectly once uploaded as the picture overlay, and it is not the fault of this conversion application.

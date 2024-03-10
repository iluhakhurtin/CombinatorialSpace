extern crate num_traits;
use num_traits::int::PrimInt;

use crate::Information;

extern crate serde;
use serde::{Serialize, Deserialize};

extern crate image;
use image::{DynamicImage, GrayImage, Luma};


/// A rule keeps information i 
/// and its interpretation int
#[derive(Serialize, Deserialize)]
pub struct Rule<T:PrimInt + Serialize> {
    pub i: Information<T>,
    pub int: Information<T>
}

impl<T:PrimInt + Serialize> Rule<T> {
    pub fn new(i: &Information<T>, int: &Information<T>) -> Rule<T> {
        let i_clone = i.clone();
        let int_clone = int.clone();
        Rule { i: i_clone, int: int_clone }
    }


    /// Prints out the rules into an image
    /// where left part is interpretation, right part is information. 
    /// This means that the image size is twice wider 
    /// than the width of i or int.
    /// White pixels show active ones.
    pub fn to_image(&self) -> DynamicImage {
        let zero = T::zero();
        let w = zero.count_zeros();
        let full_w = 2 * w;
        let h = self.i.data.len() as usize;
        let mut img = GrayImage::new(full_w, h as u32);
        for row in 0..h {
            let mut mask = T::one();
            for col in 0..full_w {
                if col == w {
                    // it is the time for i, reset the mask
                    mask = T::one();
                }

                let data_row = if col < w {
                    self.int.data[row]
                } else {
                    self.i.data[row]
                };
                
                let pixel = if (data_row & mask) > zero {
                    Luma([255])
                } else {
                    Luma([0])
                };

                img.put_pixel(col, row as u32, pixel);

                mask = mask.unsigned_shl(1);
            }
        }
        DynamicImage::ImageLuma8(img)
    }
}

#[test]
fn can_to_image() {
    let i_data = vec!(
        0b_0100000000000000,    // 0, one shift right
        0b_0000000000000000,    // 1
        0b_0000000000000000,    // 2
        0b_0000000000000000,    // 3
        0b_0000000000000000,    // 4
        0b_0000000000000000,    // 5
        0b_0000000000000000,    // 6
        0b_0000000000000000,    // 7
        0b_0000000000000000,    // 8
        0b_0000000000000000,    // 9
        0b_0000000000000000,    // 10
        0b_0000000000000000,    // 11
        0b_0000000000000000,    // 12
        0b_0000000000000000,    // 13
        0b_0000000000000000,    // 14
        0b_0000000000000000,    // 15
    );
    let i = Information::<u16> { data: i_data };

    let int_data = vec!(
        0b_1000000000000000,    // 0, initial position
        0b_0000000000000000,    // 1
        0b_0000000000000000,    // 2
        0b_0000000000000000,    // 3
        0b_0000000000000000,    // 4
        0b_0000000000000000,    // 5
        0b_0000000000000000,    // 6
        0b_0000000000000000,    // 7
        0b_0000000000000000,    // 8
        0b_0000000000000000,    // 9
        0b_0000000000000000,    // 10
        0b_0000000000000000,    // 11
        0b_0000000000000000,    // 12
        0b_0000000000000000,    // 13
        0b_0000000000000000,    // 14
        0b_0000000000000000,    // 15
    );

    let int = Information::<u16> { data: int_data };

    let r = Rule::new(&i, &int);

    let actual = r.to_image();

    let mut path = std::path::PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    path.push("files/tests/rule/can_to_image.png");
    let expected = image::open(path).unwrap();

    assert_eq!(actual, expected);
}
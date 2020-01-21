#[macro_use]
extern crate derive_more;
extern crate ndarray;
mod diffspace;
use diffspace::bitvector::BitVector;
use diffspace::code_space::CodeSpace;
use diffspace::context_map::ContextMap;
use diffspace::{consolidate, get_winner_coordinates_for_code, learn};
use rand::Rng;
use std::iter::FromIterator;

fn main() {
	let codes = diffspace::code_space::generate_code_space();

	const CONTEXT_MAP_MAX_DIM: usize = 64;
	let mut contexts = diffspace::context_map::generate_context_map(CONTEXT_MAP_MAX_DIM);

	let mut inputs: Vec<_> = codes.iter().cloned().collect();

	let mut rng = rand::thread_rng();

	let test_codes = Vec::from_iter(inputs[0..20].iter().cloned());
	// let test_codes = Vec::from_iter(inputs.iter().cloned());

	// calculate output image width and height
	let shape = contexts.shape();
	let fragment_height = shape[0] as u32 + 2; //2 is for printing the code on the bottom and separator
	let height = test_codes.len() as u32 * fragment_height;
	let width = 128;

	for step in 1.. {
		rng.shuffle(&mut inputs);

		// +++ teaching contexts map
		for code in inputs.iter() {
			learn(&mut contexts, &code);
		}

		if step % 30 == 0 {
			consolidate(&mut contexts);
		}
		// ---

		// print every n-th step
		if step % 50 == 0 {
			let mut start_x = 0;
			let mut start_y = 0;

			let mut image = image::GrayImage::new(width, height);

			for test_code in &test_codes {
				draw_winner(&contexts, test_code, &mut image, start_x, start_y);
				start_y += fragment_height;
			}

			// save image to file
			let file_name = format!("{}.png", step);
			image
				.save(format!("output/activation_maps/{}", file_name))
				.unwrap();
		}
	}
}

fn draw_winner(
	contexts: &ContextMap,
	code: &BitVector,
	image: &mut image::GrayImage,
	start_x: u32,
	start_y: u32,
) {
	// 1. Find and draw the winner
	let (y, x) = get_winner_coordinates_for_code(contexts, code);
	let winner_context = &contexts[[y, x]];
	let pixel: image::Luma<u8> = image::Luma([255]);
	let new_x = start_x + x as u32;
	let new_y = start_y + y as u32;
	image.put_pixel(new_x, new_y, pixel);

	// 2. Print the code: white is 1
	let width = 128;
	let shape = contexts.shape();
	let mut y = start_y + shape[0] as u32;
	for x in 0..width {
		let brightness = if code[x as usize] { 255 } else { 0 };

		let pixel: image::Luma<u8> = image::Luma([brightness]);
		image.put_pixel(x, y, pixel);
	}

	// 3. Print separator
	let pixel: image::Luma<u8> = image::Luma([128]);
	y += 1;
	for x in 0..width {
		image.put_pixel(x, y, pixel);
	}
}

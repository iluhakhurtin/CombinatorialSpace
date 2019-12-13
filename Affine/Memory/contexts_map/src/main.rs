#[macro_use]
extern crate derive_more;
mod diffspace;
use diffspace::bitvector::BitVector;
use diffspace::code_space::CodeSpace;
use diffspace::context_map::ContextMap;
use diffspace::learn;
use rand::Rng;

fn main() {
	let codes = diffspace::code_space::generate_code_space();
	draw_codes(&codes);
	const CONTEXT_MAP_MAX_DIM: usize = 64;
	let mut contexts = diffspace::context_map::generate_context_map(CONTEXT_MAP_MAX_DIM);

	let mut inputs: Vec<_> = codes.iter().cloned().collect();

	let mut rng = rand::thread_rng();
	let random_code_idx = rng.gen_range(0, inputs.len());
	let test_code = inputs[random_code_idx];

	for step in 1.. {
		rng.shuffle(&mut inputs);

		for code in inputs.iter() {
			learn(&mut contexts, &code);
		}

		// print every n-th step
		if step % 10000 > 0 {
			draw_contexts_activation_map(&contexts, &test_code, &step);
		}
	}
}

fn draw_codes(codes: &CodeSpace) {
	let width = 128;
	let height = codes.len() as u32;
	let mut image = image::GrayImage::new(width, height);

	let mut row = -1;
	for (code) in codes.iter() {
		row += 1;
		for i in 0..128 {
			let brightness = if code[i] { 255 } else { 0 };

			let pixel: image::Luma<u8> = image::Luma([brightness]);
			image.put_pixel(i as u32, row as u32, pixel);
		}
	}

	image.save("output/code_space.png").unwrap();
}

fn draw_contexts_activation_map(contexts: &ContextMap, code: &BitVector, step: &usize) {
	let shape = contexts.shape();
	let height = shape[0] as u32;
	let width = shape[1] as u32;

	//type GrayImage = ImageBuffer<Luma<u8>, Vec<u8>>;
	let mut image = image::GrayImage::new(width, height);

	for ((y, x), context) in contexts.indexed_iter() {
		let covariance_round = 5. * context.covariance(code).round();

		let brightness: u8 = if covariance_round > 255. {
			255
		} else {
			covariance_round as u8
		};

		let pixel: image::Luma<u8> = image::Luma([brightness]);
		image.put_pixel(x as u32, y as u32, pixel);
	}

	image
		.save(format!("output/activation_maps/{}.png", step))
		.unwrap();
}

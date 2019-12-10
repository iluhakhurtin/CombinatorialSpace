#[macro_use]
extern crate derive_more;
mod diffspace;
use diffspace::context_map::ContextMap;
use diffspace::learn;
use rand::Rng;

fn main() {
	let codes = diffspace::code_space::generate_code_space();
	const CONTEXT_MAP_MAX_DIM: usize = 64;
	let mut context_map = diffspace::context_map::generate_context_map(CONTEXT_MAP_MAX_DIM);

	let mut rng = rand::thread_rng();
	let mut inputs: Vec<_> = codes.iter().cloned().collect();

	for step in 1.. {
		rng.shuffle(&mut inputs);

		for code in inputs.iter() {
			learn(&mut context_map, *code);
		}

		// print every 100-th step
		if step % 100 > 0 {
			draw_contexts(&context_map, &step);
		}
	}
}

fn draw_contexts(context_map: &ContextMap, step: &usize) {
	type Image = image::ImageBuffer<image::Rgb<u8>, Vec<u8>>;
	// let mut image = Image::new(
	// 	1 + 2 * contexts.dim().1 as u32,
	// 	1 + 2 * contexts.dim().0 as u32,
	// );
}

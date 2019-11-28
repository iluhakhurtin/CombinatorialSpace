#[macro_use]
extern crate derive_more;
mod diffspace;
use diffspace::learn;

fn main() {
	let codes = diffspace::code_space::generate_code_space();
	const CONTEXT_MAP_MAX_DIM: usize = 64;
	let mut context_map = diffspace::context_map::generate_context_map(CONTEXT_MAP_MAX_DIM);

	let code = codes[[0, 0, 0]];
	learn(&mut context_map, &code);
}

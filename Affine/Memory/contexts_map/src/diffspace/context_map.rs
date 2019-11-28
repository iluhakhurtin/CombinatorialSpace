use super::context::Context;
use ndarray::Array2;

pub type ContextMap = Array2<Context>;

pub fn generate_context_map(max_dim: usize) -> ContextMap {
	ContextMap::from_elem((max_dim, max_dim), Context { memory: vec![] })
}

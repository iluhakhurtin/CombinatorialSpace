use super::bitvector::BitVector;

#[derive(Default, Debug, Clone)]
pub struct ContextMemoryItem {
	pub code: BitVector,
	pub hits: u32,
}

use super::context_memory_item::ContextMemoryItem;

#[derive(Default, Debug, Clone)]
pub struct Context {
	pub memory: Vec<ContextMemoryItem>,
}

use crate::{ Transformation, Information };
use crate::rule::Rule;
use std::fmt;

extern crate num_traits;
use num_traits::int::PrimInt;

extern crate serde;
use serde::{Serialize, Deserialize};

/// Context is the main module which saves the experience of 
/// transformations during the supervised learning.
/// Supervised learning is sending to the context a pair
/// of incoming information I and its appropriate interpretation
/// I_int. I and I_int is nothing more than the bytes in which
/// some bits are set. Saving such a pair context receives the 
/// rules how one set of bits is transformed into another set. 
/// On the supervised learning context saves such pairs into the
/// temporary memory. After a few iterations the memory is required
/// to be consolidated. During this process the context removes the 
/// bits that seems not to take part into the transformation.
/// Another role of the context is to apply its own transformation
/// to some new information I_new. As a result, the interpretation
/// is received: I_new_int. This piece of information is compared to
/// the shared between all context memory. If there is a match than 
/// we say that some interpretation is found.
#[derive(Serialize, Deserialize)]
pub struct Context<T: PrimInt + Serialize> {
    pub tran: Transformation,
    pub rules: Vec<Rule<T>>
}

impl<T:PrimInt + Serialize> Context<T> {
    /// Creates context with xya transformation.
    pub fn new(x: i16, y: i16, a: f32) -> Context<T> {
        let t = Transformation { x: x, y: y, a: a };
        Context::with_transformation(t)
    }

    pub fn with_transformation(t: Transformation) -> Context<T> {
        let c = Context { tran: t, rules: Vec::new() };
        c
    }

    /// This method remembers the pair i(nformation) and 
    /// its int(erpretation) for the context.
    pub fn learn(&mut self, i: &Information<T>, int: &Information<T>) {
        // for every set bit in the information i we need to remember the 
        // interpretation int with adding it to the already existing one.
        // This is done in the recursive way: 
        // 1 bit in i -> bit(s) in int instead of 1 bit in int -> bit(s) in i
        // because on the interpretation process it is required to restore
        // int by the given bits from i. It is easier to go for every bit in 
        // the given i and summarize data for int getting the resulting interpretation.

        let bits_count = T::zero().count_zeros();
        
        for data_idx in 0..i.data.len() {
            let mut mask = T::one(); // kind of 0000 0001 depending on the type
            for _ in 0..bits_count {
                // Check if incoming information has the bit set.
                if i.data[data_idx] & mask != T::zero() {
                    // We know that for now the rules have only one bit set 
                    // for the information
                    // that is why we do not need to compare anything except this 
                    // bit in the particular place via the mask. Later on this logic can be 
                    // essentially improved.
                    let rule_pos = self.rules.iter().position(|r| r.i.data[data_idx] == mask);
                    match rule_pos {
                        Some(rule_idx) => {
                            // Improve the existing rule by adding new interpretation 
                            // to the existing one with & operator. The more examples 
                            // the system gets the cleaner rule of the bit interpretation
                            // is received.
                            let rule = &self.rules[rule_idx];
                            let int_data_len = rule.int.data.len();
                            let mut new_int_data = Vec::<T>::with_capacity(int_data_len);
                            for idx in 0..int_data_len {
                                let new_d = rule.int.data[idx] & int.data[idx];
                                new_int_data.push(new_d);
                            }

                            let new_int = Information{ data: new_int_data, name: String::from("") };
                            let new_rule = Rule::new(&rule.i, &new_int);
                            
                            // update the rule
                            self.rules[rule_idx] = new_rule;
                        },
                        None => {
                            // add new rule
                            let mut i_data = vec!(T::zero(); i.data.len());
                            i_data[data_idx] = mask.clone();
                            let new_i = Information { data: i_data, name: String::from("") };
                            let new_rule = Rule::new(&new_i, &int);
                            self.rules.push(new_rule);
                        }
                    };
                }

                // shift left on one position
                mask = mask.unsigned_shl(1);
            }
        }
    }

    /// Applies all rules the context has to the incoming information i.
    pub fn interpret(&self, i: &Information<T>) -> Option<(Information<T>, f32)> {
        // on interpretation we look at every bit in the 
        // input information i and try to find a rule with the same bit set for information i as well
        // then combine all found rules interpretations int into one
        
        if self.rules.len() == 0 {
            return None;
        }

        let bit_length = T::zero().count_zeros();
        let mut d_int = vec!(T::zero(); i.data.len());

        // these two variables are used for the accuracy calculation
        let mut bits_count = 0;
        let mut match_rules_count = 0;

        for data_idx in 0..i.data.len() {
            let mut mask = T::one();

            for _ in 0..bit_length {
                // check if information has the bit set
                if i.data[data_idx] & mask != T::zero() {
                    bits_count = bits_count + 1;

                    // try to find the transformation rule with the same bit set
                    let rule = self.rules.iter().find(|r| r.i.data[data_idx] == mask);

                    match rule {
                        Some(r) => {
                            // combine the rule interpretations into one via OR
                            for i in 0..r.int.data.len() {
                                d_int[i] = d_int[i] | r.int.data[i];
                            }

                            match_rules_count = match_rules_count + 1;
                        },
                        None => {
                            // do nothing
                        }
                    }
                }

                // shift left on one position
                mask = mask.unsigned_shl(1);
            }
        }

        if bits_count == 0 || match_rules_count == 0 {
            return None;
        }

        let accuracy = match_rules_count as f32 / bits_count as f32;

        if accuracy == 0.0 || d_int.len() == 0 {
            return None;
        }

        let int = Information { data: d_int, name: String::from("") };
        Some((int, accuracy))
    }
}

impl<T: PrimInt + Serialize> fmt::Display for Context<T> {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        write!(f, "context: r cnt {}, t {}", self.rules.len(), self.tran)
    }
}
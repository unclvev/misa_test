<template>
  <label
    :class="[
      'm-radio',
      {
        'm-radio--checked': isChecked,
        'm-radio--disabled': disabled
      }
    ]"
  >
    <input
      type="radio"
      :value="value"
      :checked="isChecked"
      :disabled="disabled"
      @change="handleChange"
      class="m-radio__input"
    />
    <span class="m-radio__circle">
      <span v-if="isChecked" class="m-radio__dot"></span>
    </span>
    <span v-if="$slots.default" class="m-radio__label">
      <slot></slot>
    </span>
  </label>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  modelValue: {
    type: [String, Number, Boolean],
    default: null
  },
  value: {
    type: [String, Number, Boolean],
    required: true
  },
  disabled: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:modelValue', 'change'])

const isChecked = computed(() => props.modelValue === props.value)

const handleChange = (event) => {
  emit('update:modelValue', props.value)
  emit('change', props.value)
}
</script>

<style scoped>
.m-radio {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  user-select: none;
}

.m-radio--disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

.m-radio__input {
  position: absolute;
  opacity: 0;
  width: 0;
  height: 0;
}

.m-radio__circle {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 20px;
  height: 20px;
  border: 2px solid #9e9e9e;
  border-radius: 50%;
  background-color: #ffffff;
  transition: all 0.2s;
  flex-shrink: 0;
}

.m-radio:hover:not(.m-radio--disabled) .m-radio__circle {
  border-color: #1aa4c8;
  background-color: #e3f2fd;
  box-shadow: 0 0 0 2px rgba(26, 164, 200, 0.2);
}

.m-radio--checked .m-radio__circle {
  border-color: #1aa4c8;
  background-color: #ffffff;
}

.m-radio--checked:hover:not(.m-radio--disabled) .m-radio__circle {
  box-shadow: 0 0 0 2px rgba(26, 164, 200, 0.2);
}

.m-radio--disabled .m-radio__circle {
  border-color: #e0e0e0;
  background-color: #f5f5f5;
}

.m-radio__dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background-color: #1aa4c8;
}

.m-radio--disabled .m-radio__dot {
  background-color: #9e9e9e;
}

.m-radio__label {
  font-size: 13px;
  font-family: 'Roboto', sans-serif;
  color: #001031;
}
</style>


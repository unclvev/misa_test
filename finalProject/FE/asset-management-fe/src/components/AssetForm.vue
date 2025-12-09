<template>
  <div class="asset-form">
    <div class="asset-form__row">
      <div class="asset-form__col">
        <label class="asset-form__label">
          Mã tài sản <span class="asset-form__required">*</span>
        </label>
        <MTextField 
          v-model="localAsset.assetCode" 
          placeholder="Nhập mã tài sản"
        />
      </div>
      <div class="asset-form__col">
        <label class="asset-form__label">
          Tên tài sản <span class="asset-form__required">*</span>
        </label>
        <MTextField 
          v-model="localAsset.assetName" 
          placeholder="Nhập tên tài sản"
          :error="showAssetNameError ? assetNameError : ''"
          @blur="handleAssetNameBlur"
        />
      </div>
    </div>

    <div class="asset-form__row">
      <div class="asset-form__col">
        <label class="asset-form__label">
          Mã bộ phận sử dụng <span class="asset-form__required">*</span>
        </label>
        <MDropdown 
          v-model="localAsset.departmentCode" 
          :options="departmentOptions" 
          placeholder="Chọn mã bộ phận sử dụng"
          :error="showDepartmentCodeError ? departmentCodeError : ''"
          @blur="handleDepartmentCodeBlur"
        />
      </div>
      <div class="asset-form__col">
        <label class="asset-form__label">Tên bộ phận sử dụng</label>
        <MTextField 
          v-model="localAsset.departmentName" 
          placeholder="Nhập tên bộ phận sử dụng"
          :readonly="true"
        />
      </div>
    </div>

    <div class="asset-form__row">
      <div class="asset-form__col">
        <label class="asset-form__label">
          Mã loại tài sản <span class="asset-form__required">*</span>
        </label>
        <MDropdown 
          v-model="localAsset.assetTypeCode" 
          :options="assetTypeOptions" 
          placeholder="Chọn mã loại tài sản"
          :error="showAssetTypeCodeError ? assetTypeCodeError : ''"
          @blur="handleAssetTypeCodeBlur"
        />
      </div>
      <div class="asset-form__col">
        <label class="asset-form__label">Tên loại tài sản</label>
        <MTextField 
          v-model="localAsset.assetTypeName" 
          placeholder="Nhập tên loại tài sản"
          :readonly="true"
        />
      </div>
    </div>

    <div class="asset-form__row asset-form__row--three">
      <div class="asset-form__col">
        <label class="asset-form__label">
          Số lượng <span class="asset-form__required">*</span>
        </label>
        <MTextField 
          v-model="localAsset.quantity" 
          type="number" 
          placeholder="Nhập số lượng" 
        />
      </div>
      <div class="asset-form__col">
        <label class="asset-form__label">
          Nguyên giá <span class="asset-form__required">*</span>
        </label>
        <MTextField 
          v-model="localAsset.originalPrice" 
          type="number" 
          :show-spinner="false" 
          placeholder="Nhập nguyên giá" 
        />
      </div>
      <div class="asset-form__col">
        <label class="asset-form__label">
          Tỷ lệ hao mòn (%) <span class="asset-form__required">*</span>
        </label>
        <MTextField 
          v-model="localAsset.depreciationRate" 
          type="number" 
          :show-spinner="false" 
          placeholder="Nhập tỷ lệ hao mòn" 
        />
      </div>
    </div>

    <div class="asset-form__row asset-form__row--three">
      <div class="asset-form__col">
        <label class="asset-form__label">
          Ngày mua <span class="asset-form__required">*</span>
        </label>
        <MDatePicker 
          v-model="localAsset.purchaseDate" 
          placeholder="Chọn ngày mua" 
        />
      </div>
      <div class="asset-form__col">
        <label class="asset-form__label">
          Ngày bắt đầu sử dụng <span class="asset-form__required">*</span>
        </label>
        <MDatePicker 
          v-model="localAsset.startUsageDate" 
          placeholder="Chọn ngày bắt đầu sử dụng" 
        />
      </div>
      <div class="asset-form__col">
        <label class="asset-form__label">Năm theo dõi</label>
        <MTextField 
          v-model="localAsset.trackingYear" 
          placeholder="Nhập năm theo dõi"
          :readonly="true"
        />
      </div>
    </div>

    <div class="asset-form__row asset-form__row--last">
      <div class="asset-form__col">
        <label class="asset-form__label">
          Số năm sử dụng <span class="asset-form__required">*</span>
        </label>
        <MTextField 
          v-model="localAsset.yearsOfUse" 
          type="number" 
          placeholder="Nhập số năm sử dụng" 
        />
      </div>
      <div class="asset-form__col">
        <label class="asset-form__label">
          Giá trị hao mòn năm <span class="asset-form__required">*</span>
        </label>
        <MTextField 
          v-model="localAsset.annualDepreciationValue" 
          type="number" 
          :show-spinner="false" 
          style="text-align: right;"
        />
      </div>
      <div class="asset-form__col asset-form__col--empty"></div>
    </div>
  </div>
</template>

<script setup>
import { computed, watch, ref } from 'vue'
import MTextField from './ui/MTextField.vue'
import MDropdown from './ui/MDropdown.vue'
import MDatePicker from './ui/MDatePicker.vue'
import { useAssetForm } from '../composables/useAssetForm'

const props = defineProps({
  modelValue: {
    type: Object,
    default: () => ({})
  },
  departmentOptions: {
    type: Array,
    default: () => []
  },
  assetTypeOptions: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['update:modelValue'])

const localAsset = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

// Validation errors
const departmentCodeError = ref('')
const assetTypeCodeError = ref('')
const assetNameError = ref('')

// Track touched state - chỉ hiển thị lỗi khi đã touched hoặc khi submit
const departmentCodeTouched = ref(false)
const assetTypeCodeTouched = ref(false)
const assetNameTouched = ref(false)
const isSubmitted = ref(false)

// Computed để quyết định có hiển thị lỗi không
const showDepartmentCodeError = computed(() => {
  return (departmentCodeTouched.value || isSubmitted.value) && departmentCodeError.value !== ''
})

const showAssetTypeCodeError = computed(() => {
  return (assetTypeCodeTouched.value || isSubmitted.value) && assetTypeCodeError.value !== ''
})

const showAssetNameError = computed(() => {
  return (assetNameTouched.value || isSubmitted.value) && assetNameError.value !== ''
})

// Handlers cho blur events
const handleDepartmentCodeBlur = () => {
  departmentCodeTouched.value = true
  validateDepartmentCode()
}

const handleAssetTypeCodeBlur = () => {
  assetTypeCodeTouched.value = true
  validateAssetTypeCode()
}

const handleAssetNameBlur = () => {
  assetNameTouched.value = true
  validateAssetName()
}

// Validation functions
const validateDepartmentCode = () => {
  if (!localAsset.value.departmentCode || localAsset.value.departmentCode === '' || localAsset.value.departmentCode === null || localAsset.value.departmentCode === undefined) {
    departmentCodeError.value = 'Ô chưa được chọn'
  } else {
    departmentCodeError.value = ''
  }
}

const validateAssetTypeCode = () => {
  if (!localAsset.value.assetTypeCode || localAsset.value.assetTypeCode === '' || localAsset.value.assetTypeCode === null || localAsset.value.assetTypeCode === undefined) {
    assetTypeCodeError.value = 'Ô chưa được chọn'
  } else {
    assetTypeCodeError.value = ''
  }
}

const validateAssetName = () => {
  if (!localAsset.value.assetName || localAsset.value.assetName.trim() === '') {
    assetNameError.value = 'Tên tài sản không được để trống'
  } else {
    assetNameError.value = ''
  }
}

// Validate khi giá trị thay đổi - chỉ validate nếu đã touched hoặc đã submit
watch(() => localAsset.value.departmentCode, () => {
  if (departmentCodeTouched.value || isSubmitted.value) {
    validateDepartmentCode()
  }
})

watch(() => localAsset.value.assetTypeCode, () => {
  if (assetTypeCodeTouched.value || isSubmitted.value) {
    validateAssetTypeCode()
  }
})

watch(() => localAsset.value.assetName, () => {
  if (assetNameTouched.value || isSubmitted.value) {
    validateAssetName()
  }
})

// Expose validate method để parent có thể gọi khi submit
const validate = () => {
  isSubmitted.value = true
  validateDepartmentCode()
  validateAssetTypeCode()
  validateAssetName()
  return !departmentCodeError.value && !assetTypeCodeError.value && !assetNameError.value
}

// Reset validation state khi form được reset
const resetValidation = () => {
  departmentCodeTouched.value = false
  assetTypeCodeTouched.value = false
  assetNameTouched.value = false
  isSubmitted.value = false
  departmentCodeError.value = ''
  assetTypeCodeError.value = ''
  assetNameError.value = ''
}

// Expose methods
defineExpose({
  validate,
  resetValidation
})

// Use AssetForm composable for calculations
const {
  setupAnnualDepreciationWatcher,
  setupDefaultValuesWatcher
} = useAssetForm(localAsset, (updatedAsset) => {
  emit('update:modelValue', updatedAsset)
})

// Watch để tự động điền tên khi chọn code
watch(() => localAsset.value.departmentCode, (newCode) => {
  if (newCode && props.departmentOptions.length > 0) {
    const selectedDept = props.departmentOptions.find(opt => opt.value === newCode)
    if (selectedDept && selectedDept.name) {
      // Tự động điền tên bộ phận
      const updatedAsset = { ...localAsset.value, departmentName: selectedDept.name }
      emit('update:modelValue', updatedAsset)
    }
  }
}, { immediate: false })

watch(() => localAsset.value.assetTypeCode, (newCode) => {
  if (newCode && props.assetTypeOptions.length > 0) {
    const selectedType = props.assetTypeOptions.find(opt => opt.value === newCode)
    if (selectedType && selectedType.name) {
      // Tự động điền tên loại tài sản
      const updatedAsset = { ...localAsset.value, assetTypeName: selectedType.name }
      emit('update:modelValue', updatedAsset)
    }
  }
}, { immediate: false })

// Setup watchers từ composable
setupDefaultValuesWatcher((updatedAsset) => {
  emit('update:modelValue', updatedAsset)
})

setupAnnualDepreciationWatcher((updatedAsset) => {
  emit('update:modelValue', updatedAsset)
})
</script>

<style scoped>
.asset-form {
  display: flex;
  flex-direction: column;
  gap: 0;
}

.asset-form__row {
  display: grid;
  grid-template-columns: 1fr 2fr;
  gap: 0;
  margin-bottom: 20px; /* ⚠️ Design Spec: Vertical spacing between rows 16px - Hiện tại 20px */
}

.asset-form__row .asset-form__col:first-child {
  padding-right: 8px; /* ⚠️ Design Spec: Horizontal spacing between columns 16px - Hiện tại 8px */
}

.asset-form__row .asset-form__col:last-child {
  padding-left: 8px; /* ⚠️ Design Spec: Horizontal spacing between columns 16px - Hiện tại 8px */
}

.asset-form__row--three {
  grid-template-columns: 1fr 1fr 1fr;
  gap: 0;
}

.asset-form__row--three .asset-form__col:first-child {
  padding-right: 8px; /* ⚠️ Design Spec: Horizontal spacing between columns 16px - Hiện tại 8px */
}

.asset-form__row--three .asset-form__col:nth-child(2),
.asset-form__row--three .asset-form__col:nth-child(3) {
  padding-left: 8px; /* ⚠️ Design Spec: Horizontal spacing between columns 16px - Hiện tại 8px */
}

.asset-form__row--last {
  grid-template-columns: 1fr 1fr 1fr;
  gap: 0;
}

.asset-form__row--last .asset-form__col:first-child {
  padding-right: 8px; /* ⚠️ Design Spec: Horizontal spacing between columns 16px - Hiện tại 8px */
}

.asset-form__row--last .asset-form__col:nth-child(2),
.asset-form__row--last .asset-form__col:nth-child(3) {
  padding-left: 8px; /* ⚠️ Design Spec: Horizontal spacing between columns 16px - Hiện tại 8px */
}

.asset-form__col--empty {
  visibility: hidden;
}

.asset-form__row:last-child {
  margin-bottom: 0;
}

.asset-form__col {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.asset-form__label {
  font-size: 13px;
  font-weight: 500;
  font-family: 'Roboto', sans-serif;
  color: #001031;
}

.asset-form__required {
  color: #f44336;
}
</style>


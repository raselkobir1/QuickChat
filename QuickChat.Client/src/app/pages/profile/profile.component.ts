import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';

interface UserProfile {
  email?: string;
  fullName?: string;
  imageUrl?: string;
  coverImageUrl?: string;
  parmanantAddress?: string;
  presentAddress?: string;
  division?: string;
  district?: string;
  upazila?: string;
  universityName?: string;
  collageName?: string;
  workPlaceName?: string;
  dateOfBirth?: string | Date; // for form compatibility
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, CommonModule],
  templateUrl: './profile.component.html',
})
export class ProfileComponent implements OnInit {
  profile: UserProfile = {}; // populate from API or service
  editMode = false;
  profileForm!: FormGroup;

  constructor(private fb: FormBuilder) { }

  ngOnInit() {
    // Load user data here (replace with actual data source)
    this.profile = {
      email: 'user@example.com',
      fullName: 'Rasel Kabir',
      imageUrl: '',
      coverImageUrl: '',
      parmanantAddress: '',
      presentAddress: '',
      division: '',
      district: '',
      upazila: '',
      universityName: '',
      collageName: '',
      workPlaceName: '',
      dateOfBirth: '',
    };

    this.initForm();
  }

  initForm() {
    this.profileForm = this.fb.group({
      email: [this.profile.email, [Validators.required, Validators.email]],
      fullName: [this.profile.fullName, Validators.required],
      parmanantAddress: [this.profile.parmanantAddress],
      presentAddress: [this.profile.presentAddress],
      division: [this.profile.division],
      district: [this.profile.district],
      upazila: [this.profile.upazila],
      universityName: [this.profile.universityName],
      collageName: [this.profile.collageName],
      workPlaceName: [this.profile.workPlaceName],
      dateOfBirth: [this.formatDateForInput(this.profile.dateOfBirth)],
    });
  }

  formatDateForInput(date?: string | Date) {
    if (!date) return null;
    const d = new Date(date);
    // yyyy-MM-dd format for input type date
    return d.toISOString().split('T')[0];
  }

  toggleEdit() {
    this.editMode = !this.editMode;
    if (this.editMode) {
      this.initForm(); // Reset form values on edit start
    }
  }

  // onSave() {
  //   if (this.profileForm.invalid) return;

  //   // Save profile logic here: API call, etc.
  //   this.profile = { ...this.profile, ...this.profileForm.value };
  //   this.toggleEdit();
  // }

  async onSave() {
    if (this.profileForm.invalid) return;

    // 1. Save text data first (or combine in one API call)
    this.profile = { ...this.profile, ...this.profileForm.value };

    // 2. Upload profile image if selected
    if (this.selectedProfileImageFile) {
      // Example: upload via service, get new image URL, then update profile.imageUrl
      const uploadedUrl = await this.uploadFile(this.selectedProfileImageFile);
      this.profile.imageUrl = uploadedUrl;
      this.profileImagePreview = null;
      this.selectedProfileImageFile = undefined;
    }

    // 3. Upload cover image if selected
    if (this.selectedCoverImageFile) {
      const uploadedUrl = await this.uploadFile(this.selectedCoverImageFile);
      this.profile.coverImageUrl = uploadedUrl;
      this.coverImagePreview = null;
      this.selectedCoverImageFile = undefined;
    }

    // Finally, send updated profile to backend to save
    await this.saveProfileToBackend(this.profile);

    this.toggleEdit();
  }

  // Mock upload method (replace with your actual upload implementation)
  uploadFile(file: File): Promise<string> {
    return new Promise(resolve => {
      // simulate uploading delay
      setTimeout(() => {
        // return dummy URL or your API response URL
        resolve(URL.createObjectURL(file));
      }, 1000);
    });
  }

  // Mock save profile (replace with actual backend call)
  saveProfileToBackend(profile: any): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, 500));
  }



  // image upload functionality.
  profileImagePreview: string | ArrayBuffer | null = null;
  coverImagePreview: string | ArrayBuffer | null = null;
  selectedProfileImageFile?: File;
  selectedCoverImageFile?: File;

  onProfileImageSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedProfileImageFile = input.files[0];

      const reader = new FileReader();
      reader.onload = e => this.profileImagePreview = reader.result;
      reader.readAsDataURL(this.selectedProfileImageFile);
    }
  }

  onCoverImageSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedCoverImageFile = input.files[0];

      const reader = new FileReader();
      reader.onload = e => this.coverImagePreview = reader.result;
      reader.readAsDataURL(this.selectedCoverImageFile);
    }
  }

}
